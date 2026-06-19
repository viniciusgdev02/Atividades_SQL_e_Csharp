using MySqlConnector;

namespace Ex04_BlogSimples;

// Exercício 4 - Sistema de Blog Simples. Consome o banco "blog" via ADO.NET. Mostra o N:N entre Posts e Categorias (tabela PostCategorias) e o 1:N entre Posts e Comentários.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== BLOG =====");
            Console.WriteLine("1 - Listar posts de uma categoria (com nº de comentários)");
            Console.WriteLine("2 - Listar categorias");
            Console.WriteLine("3 - Listar posts");
            Console.WriteLine("4 - Cadastrar categoria");
            Console.WriteLine("5 - Criar post (associando categorias)");
            Console.WriteLine("6 - Adicionar comentário");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ListarPostsPorCategoria(); break;
                case "2": ListarCategorias();        break;
                case "3": ListarPosts();             break;
                case "4": CadastrarCategoria();      break;
                case "5": CriarPost();               break;
                case "6": AdicionarComentario();     break;
                case "0": executando = false;        break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: posts de uma categoria + total de comentários.
    private static void ListarPostsPorCategoria()
    {
        var categoriaId = Db.LerInt("ID da categoria: ");
        const string sql = @"
            SELECT p.titulo, p.data_publicacao, COUNT(c.id) AS total_comentarios
            FROM PostCategorias pc
            INNER JOIN Posts       p ON pc.post_id = p.id
            LEFT  JOIN Comentarios c ON c.post_id  = p.id
            WHERE pc.categoria_id = @cat
            GROUP BY p.id, p.titulo, p.data_publicacao
            ORDER BY total_comentarios DESC;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cat", categoriaId);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n--- Posts da categoria ---");
            while (reader.Read())
                Console.WriteLine($"\"{reader.GetString("titulo")}\" - " +
                                  $"{reader.GetInt64("total_comentarios")} comentário(s)");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarCategorias() =>
        ListarSimples("SELECT id, nome FROM Categorias ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")}", "Categorias");

    private static void ListarPosts() =>
        ListarSimples("SELECT id, titulo FROM Posts ORDER BY data_publicacao DESC;",
            r => $"[{r.GetInt32("id")}] {r.GetString("titulo")}", "Posts");

    private static void CadastrarCategoria()
    {
        var nome = Db.LerTexto("Nome da categoria: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand("INSERT INTO Categorias (nome) VALUES (@n);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Categoria cadastrada (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    // Cria o post e associa as categorias informadas, em uma transação.
    private static void CriarPost()
    {
        var titulo   = Db.LerTexto("Título: ");
        var conteudo = Db.LerTexto("Conteúdo: ");
        try
        {
            using var conn = Db.Abrir();
            using var tx = conn.BeginTransaction();

            using var cmdPost = new MySqlCommand(
                "INSERT INTO Posts (titulo, conteudo) VALUES (@t, @c);", conn, tx);
            cmdPost.Parameters.AddWithValue("@t", titulo);
            cmdPost.Parameters.AddWithValue("@c", conteudo);
            cmdPost.ExecuteNonQuery();
            var postId = cmdPost.LastInsertedId;

            while (true)
            {
                var catId = Db.LerInt("ID da categoria a associar (0 para finalizar): ");
                if (catId == 0) break;
                using var cmdAssoc = new MySqlCommand(
                    "INSERT INTO PostCategorias (post_id, categoria_id) VALUES (@p, @c);", conn, tx);
                cmdAssoc.Parameters.AddWithValue("@p", postId);
                cmdAssoc.Parameters.AddWithValue("@c", catId);
                cmdAssoc.ExecuteNonQuery();
            }

            tx.Commit();
            Console.WriteLine($"Post criado (id = {postId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void AdicionarComentario()
    {
        var postId   = Db.LerInt("ID do post: ");
        var autor    = Db.LerTexto("Seu nome: ");
        var conteudo = Db.LerTexto("Comentário: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Comentarios (post_id, autor, conteudo) VALUES (@p, @a, @c);", conn);
            cmd.Parameters.AddWithValue("@p", postId);
            cmd.Parameters.AddWithValue("@a", autor);
            cmd.Parameters.AddWithValue("@c", conteudo);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Comentário adicionado.");
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
