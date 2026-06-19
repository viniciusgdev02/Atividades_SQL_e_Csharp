using MySqlConnector;

namespace Ex03_ProjetosTarefas;

// Exercício 3 - Gerenciamento de Projetos e Tarefas. Consome o banco "gestao_projetos" via ADO.NET.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== PROJETOS E TAREFAS =====");
            Console.WriteLine("1 - Listar tarefas de um projeto");
            Console.WriteLine("2 - Listar usuários");
            Console.WriteLine("3 - Listar projetos");
            Console.WriteLine("4 - Cadastrar usuário");
            Console.WriteLine("5 - Cadastrar projeto");
            Console.WriteLine("6 - Criar/atribuir tarefa");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ListarTarefasDoProjeto(); break;
                case "2": ListarUsuarios();         break;
                case "3": ListarProjetos();         break;
                case "4": CadastrarUsuario();       break;
                case "5": CadastrarProjeto();       break;
                case "6": CriarTarefa();            break;
                case "0": executando = false;       break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: tarefas de um projeto com responsável e status.
    private static void ListarTarefasDoProjeto()
    {
        var projetoId = Db.LerInt("ID do projeto: ");
        const string sql = @"
            SELECT t.descricao,
                   COALESCE(u.nome, '(não atribuída)') AS responsavel,
                   t.status, t.data_vencimento
            FROM Tarefas t
            LEFT JOIN Usuarios u ON t.usuario_id = u.id
            WHERE t.projeto_id = @proj
            ORDER BY t.data_vencimento;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@proj", projetoId);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n--- Tarefas do projeto ---");
            while (reader.Read())
            {
                var venc = reader.IsDBNull(reader.GetOrdinal("data_vencimento"))
                    ? "sem prazo"
                    : reader.GetDateTime("data_vencimento").ToString("dd/MM/yyyy");
                Console.WriteLine($"- {reader.GetString("descricao")} | {reader.GetString("responsavel")} " +
                                  $"| {reader.GetString("status")} | vence: {venc}");
            }
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarUsuarios() =>
        ListarSimples("SELECT id, nome, email FROM Usuarios ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("email")}", "Usuários");

    private static void ListarProjetos() =>
        ListarSimples("SELECT id, nome FROM Projetos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")}", "Projetos");

    private static void CadastrarUsuario()
    {
        var nome  = Db.LerTexto("Nome: ");
        var email = Db.LerTexto("Email: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Usuarios (nome, email) VALUES (@n, @e);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Usuário cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarProjeto()
    {
        var nome      = Db.LerTexto("Nome do projeto: ");
        var descricao = Db.LerTexto("Descrição: ");
        var inicio    = Db.LerTexto("Data de início (AAAA-MM-DD): ");
        var fim       = Db.LerTexto("Data de fim (AAAA-MM-DD): ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Projetos (nome, descricao, data_inicio, data_fim)
                VALUES (@n, @d, @ini, @fim);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@d", descricao);
            cmd.Parameters.AddWithValue("@ini", inicio);
            cmd.Parameters.AddWithValue("@fim", fim);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Projeto cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CriarTarefa()
    {
        var projetoId = Db.LerInt("ID do projeto: ");
        var temResp   = Db.LerTexto("Atribuir a um usuário agora? (s/n): ").ToLower() == "s";
        int? usuarioId = temResp ? Db.LerInt("ID do usuário: ") : null;
        var descricao = Db.LerTexto("Descrição da tarefa: ");
        var venc      = Db.LerTexto("Data de vencimento (AAAA-MM-DD): ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Tarefas (projeto_id, usuario_id, descricao, status, data_vencimento)
                VALUES (@proj, @user, @desc, 'a fazer', @venc);", conn);
            cmd.Parameters.AddWithValue("@proj", projetoId);
            // Quando não há responsável, gravamos NULL na coluna usuario_id.
            cmd.Parameters.AddWithValue("@user", (object?)usuarioId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@desc", descricao);
            cmd.Parameters.AddWithValue("@venc", venc);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Tarefa criada (id = {cmd.LastInsertedId}).");
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
