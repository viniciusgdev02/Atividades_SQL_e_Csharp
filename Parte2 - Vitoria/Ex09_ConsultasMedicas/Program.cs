using MySqlConnector;

namespace Ex09_ConsultasMedicas;

// Exercício 9 - Gerenciamento de Consultas Médicas. Consome o banco "clinica" via ADO.NET.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== CONSULTAS MÉDICAS =====");
            Console.WriteLine("1 - Listar consultas de um médico em uma data");
            Console.WriteLine("2 - Listar médicos");
            Console.WriteLine("3 - Listar pacientes");
            Console.WriteLine("4 - Cadastrar paciente");
            Console.WriteLine("5 - Cadastrar médico");
            Console.WriteLine("6 - Agendar consulta");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ConsultasDoMedicoNaData(); break;
                case "2": ListarMedicos();           break;
                case "3": ListarPacientes();         break;
                case "4": CadastrarPaciente();       break;
                case "5": CadastrarMedico();         break;
                case "6": AgendarConsulta();         break;
                case "0": executando = false;        break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: consultas de um médico em uma data específica.
    private static void ConsultasDoMedicoNaData()
    {
        var medicoId = Db.LerInt("ID do médico: ");
        var data     = Db.LerTexto("Data (AAAA-MM-DD): ");
        const string sql = @"
            SELECT c.data_hora, p.nome AS paciente, c.status, c.observacoes
            FROM Consultas c
            INNER JOIN Pacientes p ON c.paciente_id = p.id
            WHERE c.medico_id = @med
              AND DATE(c.data_hora) = @data
            ORDER BY c.data_hora;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@med", medicoId);
            cmd.Parameters.AddWithValue("@data", data);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Consultas do dia ---");
            var algum = false;
            while (reader.Read())
            {
                algum = true;
                var obs = reader.IsDBNull(reader.GetOrdinal("observacoes")) ? "" : $" - {reader.GetString("observacoes")}";
                Console.WriteLine($"{reader.GetDateTime("data_hora"):HH:mm} | {reader.GetString("paciente")} " +
                                  $"| {reader.GetString("status")}{obs}");
            }
            if (!algum) Console.WriteLine("Nenhuma consulta para esse médico nessa data.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarMedicos() =>
        ListarSimples("SELECT id, nome, especialidade FROM Medicos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("especialidade")}", "Médicos");

    private static void ListarPacientes() =>
        ListarSimples("SELECT id, nome, telefone FROM Pacientes ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("telefone")}", "Pacientes");

    private static void CadastrarPaciente()
    {
        var nome     = Db.LerTexto("Nome: ");
        var nasc     = Db.LerTexto("Data de nascimento (AAAA-MM-DD): ");
        var telefone = Db.LerTexto("Telefone: ");
        var endereco = Db.LerTexto("Endereço: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Pacientes (nome, data_nascimento, telefone, endereco)
                VALUES (@n, @nasc, @tel, @end);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@nasc", nasc);
            cmd.Parameters.AddWithValue("@tel", telefone);
            cmd.Parameters.AddWithValue("@end", endereco);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Paciente cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarMedico()
    {
        var nome = Db.LerTexto("Nome: ");
        var esp  = Db.LerTexto("Especialidade: ");
        var crm  = Db.LerTexto("CRM: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Medicos (nome, especialidade, CRM) VALUES (@n, @e, @crm);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", esp);
            cmd.Parameters.AddWithValue("@crm", crm);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Médico cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine("[AVISO] Já existe um médico com esse CRM.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void AgendarConsulta()
    {
        var pacienteId = Db.LerInt("ID do paciente: ");
        var medicoId   = Db.LerInt("ID do médico: ");
        var dataHora   = Db.LerTexto("Data e hora (AAAA-MM-DD HH:MM): ");
        var obs        = Db.LerTexto("Observações: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Consultas (paciente_id, medico_id, data_hora, status, observacoes)
                VALUES (@p, @m, @dh, 'agendada', @obs);", conn);
            cmd.Parameters.AddWithValue("@p", pacienteId);
            cmd.Parameters.AddWithValue("@m", medicoId);
            cmd.Parameters.AddWithValue("@dh", dataHora);
            cmd.Parameters.AddWithValue("@obs", string.IsNullOrEmpty(obs) ? (object)DBNull.Value : obs);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Consulta agendada (id = {cmd.LastInsertedId}).");
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
