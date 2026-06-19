using MySqlConnector;

namespace Ex01_GerenciamentoLivros;

// Exercício 1 - Sistema de Gerenciamento de Livros. Aplicativo de console que consome o banco "biblioteca" via ADO.NET.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== GERENCIAMENTO DE LIVROS =====");
            Console.WriteLine("1 - Listar livros (com autor e editora)");
            Console.WriteLine("2 - Listar autores");
            Console.WriteLine("3 - Listar editoras");
            Console.WriteLine("4 - Cadastrar autor");
            Console.WriteLine("5 - Cadastrar editora");
            Console.WriteLine("6 - Cadastrar livro");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ListarLivros();     break;
                case "2": ListarAutores();    break;
                case "3": ListarEditoras();   break;
                case "4": CadastrarAutor();   break;
                case "5": CadastrarEditora(); break;
                case "6": CadastrarLivro();   break;
                case "0": executando = false; break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida no enunciado: livros + autor + editora.
    private static void ListarLivros()
    {
        const string sql = @"
            SELECT l.titulo, l.ano_publicacao, a.nome AS autor, e.nome AS editora
            FROM Livros l
            INNER JOIN Autores  a ON l.autor_id   = a.id
            INNER JOIN Editoras e ON l.editora_id = e.id
            ORDER BY a.nome, l.titulo;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n--- Livros cadastrados ---");
            while (reader.Read())
            {
                Console.WriteLine($"\"{reader.GetString("titulo")}\" ({reader.GetInt32("ano_publicacao")}) " +
                                  $"- {reader.GetString("autor")} | {reader.GetString("editora")}");
            }
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarAutores()
    {
        ListarSimples("SELECT id, nome, nacionalidade FROM Autores ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} ({r.GetString("nacionalidade")})",
            "Autores");
    }

    private static void ListarEditoras()
    {
        ListarSimples("SELECT id, nome, cidade FROM Editoras ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("cidade")}",
            "Editoras");
    }

    private static void CadastrarAutor()
    {
        var nome = Db.LerTexto("Nome do autor: ");
        var nac  = Db.LerTexto("Nacionalidade: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Autores (nome, nacionalidade) VALUES (@nome, @nac);", conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@nac", nac);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Autor cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarEditora()
    {
        var nome   = Db.LerTexto("Nome da editora: ");
        var cidade = Db.LerTexto("Cidade: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Editoras (nome, cidade) VALUES (@nome, @cidade);", conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@cidade", cidade);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Editora cadastrada (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarLivro()
    {
        var titulo    = Db.LerTexto("Título do livro: ");
        var ano       = Db.LerInt("Ano de publicação: ");
        var autorId   = Db.LerInt("ID do autor: ");
        var editoraId = Db.LerInt("ID da editora: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Livros (titulo, ano_publicacao, autor_id, editora_id)
                VALUES (@titulo, @ano, @autor, @editora);", conn);
            cmd.Parameters.AddWithValue("@titulo", titulo);
            cmd.Parameters.AddWithValue("@ano", ano);
            cmd.Parameters.AddWithValue("@autor", autorId);
            cmd.Parameters.AddWithValue("@editora", editoraId);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Livro cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    // ----------------------- Auxiliares -----------------------

    // Executa um SELECT simples e imprime cada linha via formatador.
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

    private static void Erro(Exception ex) =>
        Console.WriteLine($"[ERRO] {ex.Message}");
}
