# Parte 2 — Exercícios 6 a 10 (Sistema em C# + MySQL)

Solution .NET 8 com **5 aplicativos de console** (um por exercício), cada um
consumindo seu próprio banco MySQL via **ADO.NET puro (MySqlConnector)**.
Esta é a metade do trabalho em dupla (Exercícios 6 a 10).

## Stack

- **C# / .NET 8** (aplicativos de console)
- **MySqlConnector** (driver ADO.NET) — acesso a dados sem ORM
- **MySQL 8.0+** ou **MariaDB 10.4+**

## Estrutura

```
Parte2/
├── Parte2.sln                       # abre os 5 projetos juntos
├── database/                        # scripts SQL (criam os bancos)
│   ├── ex06_inventario_loja.sql
│   ├── ex07_eventos_participantes.sql
│   ├── ex08_avaliacao_filmes.sql
│   ├── ex09_consultas_medicas.sql
│   └── ex10_gerenciamento_restaurantes.sql
├── Ex06_InventarioLoja/
│   ├── Ex06_InventarioLoja.csproj
│   ├── Db.cs                         # conexão + utilitários de console
│   └── Program.cs                    # menu + operações
├── Ex07_EventosParticipantes/
├── Ex08_AvaliacaoFilmes/
├── Ex09_ConsultasMedicas/
└── Ex10_GerenciamentoRestaurantes/
```

| Projeto | Exercício | Banco |
|---|---|---|
| Ex06_InventarioLoja | 6 | `inventario_loja` |
| Ex07_EventosParticipantes | 7 | `eventos` |
| Ex08_AvaliacaoFilmes | 8 | `avaliacao_filmes` |
| Ex09_ConsultasMedicas | 9 | `clinica` |
| Ex10_GerenciamentoRestaurantes | 10 | `restaurante` |

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Servidor MySQL/MariaDB rodando localmente
- Conexão com a internet na primeira execução (para o `dotnet` baixar o
  pacote NuGet `MySqlConnector`)

## Passo 1 — Criar os bancos

```bash
cd database
mysql -u root -p < ex06_inventario_loja.sql
mysql -u root -p < ex07_eventos_participantes.sql
mysql -u root -p < ex08_avaliacao_filmes.sql
mysql -u root -p < ex09_consultas_medicas.sql
mysql -u root -p < ex10_gerenciamento_restaurantes.sql
```

## Passo 2 — Configurar a conexão

Cada projeto tem um `Db.cs` com a string de conexão padrão:

```
Server=localhost;Port=3306;Database=<banco>;User ID=root;Password=;
```

Ajuste usuário/senha em `Db.cs` ou defina a variável de ambiente `MYSQL_CONN`
(tem prioridade sobre o valor padrão):

```bash
# Linux/macOS
export MYSQL_CONN="Server=localhost;Database=inventario_loja;User ID=root;Password=minhasenha;"

# Windows (PowerShell)
$env:MYSQL_CONN="Server=localhost;Database=inventario_loja;User ID=root;Password=minhasenha;"
```

## Passo 3 — Compilar e executar

```bash
dotnet restore Parte2.sln
dotnet run --project Ex06_InventarioLoja
```

Cada aplicativo abre um **menu** no terminal: listar (consulta pedida),
cadastrar registros e sair.

## Detalhe de cada sistema

- **Ex06 — Inventário:** mostra o estoque atual de cada produto, **calculado**
  a partir das movimentações (entradas − saídas); cadastra fornecedores,
  produtos e registra movimentações (entrada/saída).
- **Ex07 — Eventos:** lista os participantes de um evento que já pagaram;
  cadastra eventos/participantes e faz inscrições (trata inscrição duplicada).
- **Ex08 — Filmes:** mostra a média de avaliação e os comentários de um filme;
  cadastra filmes/usuários e registra avaliações (um usuário só avalia cada
  filme uma vez — restrição `UNIQUE`, tratada com aviso).
- **Ex09 — Consultas:** lista as consultas de um médico em uma data; cadastra
  pacientes/médicos (CRM único) e agenda consultas.
- **Ex10 — Restaurante:** lista os ingredientes (e quantidades) de um prato;
  cadastra ingredientes e cria pratos com sua receita (N:N) em transação.

## Notas técnicas

- Queries com **parâmetros** (`@nome`), protegendo contra SQL Injection.
- Conexões abertas com `using` (devolvidas ao pool ao final).
- O estoque do Ex06 é a "fonte única de verdade" nas transações — não há coluna
  de saldo que possa ficar desatualizada.
- Erros de duplicidade (código MySQL **1062**) são tratados com mensagem clara.
