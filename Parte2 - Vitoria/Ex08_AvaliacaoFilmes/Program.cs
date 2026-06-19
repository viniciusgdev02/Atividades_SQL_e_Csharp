using MySqlConnector;

namespace Ex08_AvaliacaoFilmes;

// Exercício 8 - Sistema de Avaliação de Filmes. Consome o banco "avaliacao_filmes" via ADO.NET. A tabela Avaliacoes é a junção N:N entre Usuarios e Filmes.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== AVALIAÇÃO DE FILMES =====");
            Console.WriteLine("1 - Ver média e comentários de um filme");
            Console.WriteLine("2 - Listar filmes");
            Console.WriteLine("3 - Listar usuários");
            Console.WriteLine("4 - Cadastrar filme");
            Console.WriteLine("5 - Cadastrar usuário");
            Console.WriteLine("6 - Avaliar filme");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": MediaEComentarios();  break;
                case "2": ListarFilmes();       break;
                case "3": ListarUsuarios();     break;
                case "4": CadastrarFilme();     break;
                case "5": CadastrarUsuario();   break;
                case "6": Avaliar();            break;
                case "0": executando = false;   break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: média de avaliação + comentários de um filme.
    private static void MediaEComentarios()
    {
        var filmeId = Db.LerInt("ID do filme: ");
        try
        {
            using var conn = Db.Abrir();

            // (A) média e total de avaliações
            using (var cmdMedia = new MySqlCommand(@"
                SELECT f.titulo, ROUND(AVG(a.nota), 2) AS media, COUNT(a.id) AS total
                FROM Filmes f
                LEFT JOIN Avaliacoes a ON a.filme_id = f.id
                WHERE f.id = @f
                GROUP BY f.id, f.titulo;", conn))
            {
                cmdMedia.Parameters.AddWithValue("@f", filmeId);
                using var r = cmdMedia.ExecuteReader();
                if (r.Read())
                {
                    var media = r.IsDBNull(r.GetOrdinal("media")) ? "-" : r.GetDecimal("media").ToString("0.00");
                    Console.WriteLine($"\n{r.GetString("titulo")}: média {media} ({r.GetInt64("total")} avaliações)");
                }
                else
                {
                    Console.WriteLine("Filme não encontrado.");
                    return;
                }
            }

            // (B) comentários do filme
            using var cmdCom = new MySqlCommand(@"
                SELECT u.nome, a.nota, a.comentario, a.data_avaliacao
                FROM Avaliacoes a
                INNER JOIN Usuarios u ON a.usuario_id = u.id
                WHERE a.filme_id = @f
                ORDER BY a.data_avaliacao DESC;", conn);
            cmdCom.Parameters.AddWithValue("@f", filmeId);
            using var rc = cmdCom.ExecuteReader();
            Console.WriteLine("--- Comentários ---");
            while (rc.Read())
            {
                var coment = rc.IsDBNull(rc.GetOrdinal("comentario")) ? "" : rc.GetString("comentario");
                Console.WriteLine($"[{rc.GetDecimal("nota"):0.0}] {rc.GetString("nome")}: {coment}");
            }
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarFilmes() =>
        ListarSimples("SELECT id, titulo, ano_lancamento FROM Filmes ORDER BY titulo;",
            r => $"[{r.GetInt32("id")}] {r.GetString("titulo")} ({r.GetInt32("ano_lancamento")})", "Filmes");

    private static void ListarUsuarios() =>
        ListarSimples("SELECT id, nome FROM Usuarios ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")}", "Usuários");

    private static void CadastrarFilme()
    {
        var titulo = Db.LerTexto("Título: ");
        var ano    = Db.LerInt("Ano de lançamento: ");
        var dir    = Db.LerTexto("Diretor: ");
        var genero = Db.LerTexto("Gênero: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Filmes (titulo, ano_lancamento, diretor, genero)
                VALUES (@t, @a, @d, @g);", conn);
            cmd.Parameters.AddWithValue("@t", titulo);
            cmd.Parameters.AddWithValue("@a", ano);
            cmd.Parameters.AddWithValue("@d", dir);
            cmd.Parameters.AddWithValue("@g", genero);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Filme cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarUsuario()
    {
        var nome  = Db.LerTexto("Nome: ");
        var email = Db.LerTexto("Email: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand("INSERT INTO Usuarios (nome, email) VALUES (@n, @e);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Usuário cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void Avaliar()
    {
        var usuarioId = Db.LerInt("ID do usuário: ");
        var filmeId   = Db.LerInt("ID do filme: ");
        var nota      = Db.LerDecimal("Nota (0 a 10): ");
        var coment    = Db.LerTexto("Comentário: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Avaliacoes (usuario_id, filme_id, nota, comentario)
                VALUES (@u, @f, @n, @c);", conn);
            cmd.Parameters.AddWithValue("@u", usuarioId);
            cmd.Parameters.AddWithValue("@f", filmeId);
            cmd.Parameters.AddWithValue("@n", nota);
            cmd.Parameters.AddWithValue("@c", coment);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Avaliação registrada.");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // UNIQUE (usuario_id, filme_id): usuário só avalia cada filme uma vez.
            Console.WriteLine("[AVISO] Esse usuário já avaliou esse filme.");
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
