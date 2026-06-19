using MySqlConnector;

namespace Ex02_PedidosOnline;

// Exercício 2 - Registro de Pedidos Online. Consome o banco "loja_online" via ADO.NET. Demonstra o relacionamento N:N entre Pedidos e Produtos através da tabela ItensPedido.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== PEDIDOS ONLINE =====");
            Console.WriteLine("1 - Listar pedidos de um cliente (com itens)");
            Console.WriteLine("2 - Listar clientes");
            Console.WriteLine("3 - Listar produtos");
            Console.WriteLine("4 - Cadastrar cliente");
            Console.WriteLine("5 - Cadastrar produto");
            Console.WriteLine("6 - Criar pedido (com itens)");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ListarPedidosDoCliente(); break;
                case "2": ListarClientes();         break;
                case "3": ListarProdutos();         break;
                case "4": CadastrarCliente();       break;
                case "5": CadastrarProduto();       break;
                case "6": CriarPedido();            break;
                case "0": executando = false;       break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: pedidos de um cliente com produtos e quantidades.
    private static void ListarPedidosDoCliente()
    {
        var clienteId = Db.LerInt("ID do cliente: ");
        const string sql = @"
            SELECT p.id AS pedido, p.data_pedido, p.status,
                   pr.nome AS produto, ip.quantidade, ip.preco_unitario,
                   (ip.quantidade * ip.preco_unitario) AS subtotal
            FROM Pedidos p
            INNER JOIN ItensPedido ip ON p.id = ip.pedido_id
            INNER JOIN Produtos    pr ON ip.produto_id = pr.id
            WHERE p.cliente_id = @cliente
            ORDER BY p.id;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cliente", clienteId);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n--- Pedidos do cliente ---");
            var algum = false;
            while (reader.Read())
            {
                algum = true;
                Console.WriteLine($"Pedido #{reader.GetInt32("pedido")} [{reader.GetString("status")}] " +
                                  $"{reader.GetString("produto")} x{reader.GetInt32("quantidade")} " +
                                  $"= R$ {reader.GetDecimal("subtotal"):0.00}");
            }
            if (!algum) Console.WriteLine("Nenhum pedido encontrado para esse cliente.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarClientes() =>
        ListarSimples("SELECT id, nome, email FROM Clientes ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("email")}", "Clientes");

    private static void ListarProdutos() =>
        ListarSimples("SELECT id, nome, preco, estoque FROM Produtos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - R$ {r.GetDecimal("preco"):0.00} (estoque: {r.GetInt32("estoque")})",
            "Produtos");

    private static void CadastrarCliente()
    {
        var nome     = Db.LerTexto("Nome: ");
        var email    = Db.LerTexto("Email: ");
        var endereco = Db.LerTexto("Endereço: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Clientes (nome, email, endereco) VALUES (@n, @e, @end);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@end", endereco);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Cliente cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarProduto()
    {
        var nome    = Db.LerTexto("Nome: ");
        var preco   = Db.LerDecimal("Preço: ");
        var estoque = Db.LerInt("Estoque inicial: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Produtos (nome, preco, estoque) VALUES (@n, @p, @e);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@p", preco);
            cmd.Parameters.AddWithValue("@e", estoque);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Produto cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    // cria o pedido + os itens numa transacao: ou grava tudo ou nada
    private static void CriarPedido()
    {
        var clienteId = Db.LerInt("ID do cliente: ");
        try
        {
            using var conn = Db.Abrir();
            using var tx = conn.BeginTransaction();

            // 1) cria o cabeçalho do pedido
            using var cmdPedido = new MySqlCommand(
                "INSERT INTO Pedidos (cliente_id, status) VALUES (@c, 'pendente');", conn, tx);
            cmdPedido.Parameters.AddWithValue("@c", clienteId);
            cmdPedido.ExecuteNonQuery();
            var pedidoId = cmdPedido.LastInsertedId;

            // 2) adiciona itens até o usuário digitar 0
            while (true)
            {
                var produtoId = Db.LerInt("ID do produto (0 para finalizar): ");
                if (produtoId == 0) break;
                var qtd   = Db.LerInt("Quantidade: ");
                var preco = Db.LerDecimal("Preço unitário: ");

                using var cmdItem = new MySqlCommand(@"
                    INSERT INTO ItensPedido (pedido_id, produto_id, quantidade, preco_unitario)
                    VALUES (@ped, @prod, @qtd, @preco);", conn, tx);
                cmdItem.Parameters.AddWithValue("@ped", pedidoId);
                cmdItem.Parameters.AddWithValue("@prod", produtoId);
                cmdItem.Parameters.AddWithValue("@qtd", qtd);
                cmdItem.Parameters.AddWithValue("@preco", preco);
                cmdItem.ExecuteNonQuery();
            }

            tx.Commit();
            Console.WriteLine($"Pedido criado (id = {pedidoId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarSimples(string sql, Func<MySqlDataReader, string> formato, string titulo)
    {
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine($"\n--- {titulo} ---");
            while (reader.Read())
                Console.WriteLine(formato(reader));
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void Erro(Exception ex) => Console.WriteLine($"[ERRO] {ex.Message}");
}
