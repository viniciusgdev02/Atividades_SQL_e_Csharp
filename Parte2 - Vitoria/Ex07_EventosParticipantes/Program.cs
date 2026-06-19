using MySqlConnector;

namespace Ex07_EventosParticipantes;

// Exercício 7 - Sistema de Eventos e Participantes. Consome o banco "eventos" via ADO.NET. A tabela Inscricoes é a junção N:N entre Eventos e Participantes.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== EVENTOS E PARTICIPANTES =====");
            Console.WriteLine("1 - Listar participantes que já pagaram (de um evento)");
            Console.WriteLine("2 - Listar eventos");
            Console.WriteLine("3 - Listar participantes");
            Console.WriteLine("4 - Cadastrar evento");
            Console.WriteLine("5 - Cadastrar participante");
            Console.WriteLine("6 - Inscrever participante em evento");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": ParticipantesPagantes(); break;
                case "2": ListarEventos();         break;
                case "3": ListarParticipantes();   break;
                case "4": CadastrarEvento();       break;
                case "5": CadastrarParticipante(); break;
                case "6": Inscrever();             break;
                case "0": executando = false;      break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: participantes de um evento com pagamento confirmado.
    private static void ParticipantesPagantes()
    {
        var eventoId = Db.LerInt("ID do evento: ");
        const string sql = @"
            SELECT p.nome, p.email, i.data_inscricao
            FROM Inscricoes i
            INNER JOIN Participantes p ON i.participante_id = p.id
            WHERE i.evento_id = @ev
              AND i.status_pagamento = 'pago'
            ORDER BY p.nome;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ev", eventoId);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Participantes que pagaram ---");
            var algum = false;
            while (reader.Read())
            {
                algum = true;
                Console.WriteLine($"- {reader.GetString("nome")} ({reader.GetString("email")}) " +
                                  $"| inscrito em {reader.GetDateTime("data_inscricao"):dd/MM/yyyy}");
            }
            if (!algum) Console.WriteLine("Nenhum participante pagante encontrado.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarEventos() =>
        ListarSimples("SELECT id, nome, data_evento FROM Eventos ORDER BY data_evento;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetDateTime("data_evento"):dd/MM/yyyy}", "Eventos");

    private static void ListarParticipantes() =>
        ListarSimples("SELECT id, nome, email FROM Participantes ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("email")}", "Participantes");

    private static void CadastrarEvento()
    {
        var nome      = Db.LerTexto("Nome do evento: ");
        var data      = Db.LerTexto("Data (AAAA-MM-DD): ");
        var local     = Db.LerTexto("Local: ");
        var descricao = Db.LerTexto("Descrição: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Eventos (nome, data_evento, local, descricao)
                VALUES (@n, @d, @l, @desc);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@d", data);
            cmd.Parameters.AddWithValue("@l", local);
            cmd.Parameters.AddWithValue("@desc", descricao);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Evento cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarParticipante()
    {
        var nome     = Db.LerTexto("Nome: ");
        var email    = Db.LerTexto("Email: ");
        var telefone = Db.LerTexto("Telefone: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Participantes (nome, email, telefone) VALUES (@n, @e, @t);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@t", telefone);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Participante cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void Inscrever()
    {
        var eventoId       = Db.LerInt("ID do evento: ");
        var participanteId = Db.LerInt("ID do participante: ");
        var status         = Db.LerTexto("Status do pagamento (pago/pendente/cancelado): ").ToLower();
        if (status != "pago" && status != "pendente" && status != "cancelado")
            status = "pendente";
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Inscricoes (evento_id, participante_id, status_pagamento)
                VALUES (@ev, @p, @s);", conn);
            cmd.Parameters.AddWithValue("@ev", eventoId);
            cmd.Parameters.AddWithValue("@p", participanteId);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Inscrição registrada.");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine("[AVISO] Esse participante já está inscrito nesse evento.");
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
