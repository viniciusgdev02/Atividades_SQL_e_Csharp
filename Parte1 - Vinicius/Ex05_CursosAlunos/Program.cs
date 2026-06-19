using MySqlConnector;

namespace Ex05_CursosAlunos;

// Exercício 5 - Gerenciamento de Cursos e Alunos. Consome o banco "plataforma_cursos" via ADO.NET. A tabela Matriculas é a junção N:N entre Alunos e Cursos.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== CURSOS E ALUNOS =====");
            Console.WriteLine("1 - Listar cursos de um aluno");
            Console.WriteLine("2 - Listar alunos de um curso");
            Console.WriteLine("3 - Listar alunos");
            Console.WriteLine("4 - Listar cursos");
            Console.WriteLine("5 - Cadastrar aluno");
            Console.WriteLine("6 - Cadastrar curso");
            Console.WriteLine("7 - Matricular aluno em curso");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": CursosDoAluno();    break;
                case "2": AlunosDoCurso();    break;
                case "3": ListarAlunos();     break;
                case "4": ListarCursos();     break;
                case "5": CadastrarAluno();   break;
                case "6": CadastrarCurso();   break;
                case "7": Matricular();       break;
                case "0": executando = false; break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida (A): cursos em que um aluno está matriculado.
    private static void CursosDoAluno()
    {
        var alunoId = Db.LerInt("ID do aluno: ");
        const string sql = @"
            SELECT c.titulo, c.instrutor, m.data_matricula, m.status
            FROM Matriculas m
            INNER JOIN Cursos c ON m.curso_id = c.id
            WHERE m.aluno_id = @aluno
            ORDER BY m.data_matricula;";

        ExecutarLista(sql, "@aluno", alunoId, r =>
            $"- {r.GetString("titulo")} ({r.GetString("instrutor")}) | {r.GetString("status")}",
            "Cursos do aluno");
    }

    // Consulta pedida (B): alunos matriculados em um curso.
    private static void AlunosDoCurso()
    {
        var cursoId = Db.LerInt("ID do curso: ");
        const string sql = @"
            SELECT a.nome, a.email, m.data_matricula, m.status
            FROM Matriculas m
            INNER JOIN Alunos a ON m.aluno_id = a.id
            WHERE m.curso_id = @curso
            ORDER BY a.nome;";

        ExecutarLista(sql, "@curso", cursoId, r =>
            $"- {r.GetString("nome")} ({r.GetString("email")}) | {r.GetString("status")}",
            "Alunos do curso");
    }

    private static void ListarAlunos() =>
        ListarSimples("SELECT id, nome, email FROM Alunos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("email")}", "Alunos");

    private static void ListarCursos() =>
        ListarSimples("SELECT id, titulo, instrutor FROM Cursos ORDER BY titulo;",
            r => $"[{r.GetInt32("id")}] {r.GetString("titulo")} - {r.GetString("instrutor")}", "Cursos");

    private static void CadastrarAluno()
    {
        var nome  = Db.LerTexto("Nome: ");
        var email = Db.LerTexto("Email: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand("INSERT INTO Alunos (nome, email) VALUES (@n, @e);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Aluno cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarCurso()
    {
        var titulo    = Db.LerTexto("Título: ");
        var descricao = Db.LerTexto("Descrição: ");
        var instrutor = Db.LerTexto("Instrutor: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Cursos (titulo, descricao, instrutor) VALUES (@t, @d, @i);", conn);
            cmd.Parameters.AddWithValue("@t", titulo);
            cmd.Parameters.AddWithValue("@d", descricao);
            cmd.Parameters.AddWithValue("@i", instrutor);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Curso cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void Matricular()
    {
        var alunoId = Db.LerInt("ID do aluno: ");
        var cursoId = Db.LerInt("ID do curso: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Matriculas (aluno_id, curso_id, status) VALUES (@a, @c, 'ativa');", conn);
            cmd.Parameters.AddWithValue("@a", alunoId);
            cmd.Parameters.AddWithValue("@c", cursoId);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Matrícula realizada.");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // 1062 = chave duplicada (aluno ja matriculado nesse curso)
            Console.WriteLine("[AVISO] Esse aluno já está matriculado nesse curso.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    // ----------------------- Auxiliares -----------------------

    private static void ExecutarLista(string sql, string param, int valor,
        Func<MySqlDataReader, string> formato, string titulo)
    {
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue(param, valor);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine($"\n--- {titulo} ---");
            var algum = false;
            while (reader.Read()) { algum = true; Console.WriteLine(formato(reader)); }
            if (!algum) Console.WriteLine("Nenhum registro encontrado.");
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
