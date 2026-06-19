# Parte 1 — Exercícios 1 a 5 (Sistema em C# + MySQL)

Solution .NET 8 com **5 aplicativos de console** (um por exercício), cada um
consumindo seu próprio banco MySQL via **ADO.NET puro (MySqlConnector)**.
Esta é a metade do trabalho em dupla (Exercícios 1 a 5).

## Stack

- **C# / .NET 8** (aplicativos de console)
- **MySqlConnector** (driver ADO.NET) — acesso a dados sem ORM
- **MySQL 8.0+** ou **MariaDB 10.4+**

## Estrutura

```
Parte1/
├── Parte1.sln                       # abre os 5 projetos juntos
├── database/                        # scripts SQL (criam os bancos)
│   ├── ex01_gerenciamento_livros.sql
│   ├── ex02_pedidos_online.sql
│   ├── ex03_projetos_tarefas.sql
│   ├── ex04_blog_simples.sql
│   └── ex05_cursos_alunos.sql
├── Ex01_GerenciamentoLivros/
│   ├── Ex01_GerenciamentoLivros.csproj
│   ├── Db.cs                         # conexão + utilitários de console
│   └── Program.cs                    # menu + operações
├── Ex02_PedidosOnline/
├── Ex03_ProjetosTarefas/
├── Ex04_BlogSimples/
└── Ex05_CursosAlunos/
```

| Projeto | Exercício | Banco |
|---|---|---|
| Ex01_GerenciamentoLivros | 1 | `biblioteca` |
| Ex02_PedidosOnline | 2 | `loja_online` |
| Ex03_ProjetosTarefas | 3 | `gestao_projetos` |
| Ex04_BlogSimples | 4 | `blog` |
| Ex05_CursosAlunos | 5 | `plataforma_cursos` |

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Servidor MySQL/MariaDB rodando localmente
- Conexão com a internet na primeira execução (para o `dotnet` baixar o
  pacote NuGet `MySqlConnector`)

## Passo 1 — Criar os bancos

Execute os scripts da pasta `database/` (cada um cria seu banco do zero):

```bash
cd database
mysql -u root -p < ex01_gerenciamento_livros.sql
mysql -u root -p < ex02_pedidos_online.sql
mysql -u root -p < ex03_projetos_tarefas.sql
mysql -u root -p < ex04_blog_simples.sql
mysql -u root -p < ex05_cursos_alunos.sql
```

> Também é possível abrir cada `.sql` no MySQL Workbench/DBeaver e executar.

## Passo 2 — Configurar a conexão

Cada projeto tem um arquivo `Db.cs` com a string de conexão padrão:

```
Server=localhost;Port=3306;Database=<banco>;User ID=root;Password=;
```

Ajuste **usuário e senha** conforme o seu ambiente. Você pode editar a
constante `ConexaoPadrao` em `Db.cs` ou, sem mexer no código, definir a
variável de ambiente `MYSQL_CONN` (ela tem prioridade):

```bash
# Linux/macOS
export MYSQL_CONN="Server=localhost;Database=biblioteca;User ID=root;Password=minhasenha;"

# Windows (PowerShell)
$env:MYSQL_CONN="Server=localhost;Database=biblioteca;User ID=root;Password=minhasenha;"
```

## Passo 3 — Compilar e executar

Restaurar pacotes uma vez:

```bash
dotnet restore Parte1.sln
```

Executar um exercício específico (exemplo: Exercício 1):

```bash
dotnet run --project Ex01_GerenciamentoLivros
```

Cada aplicativo abre um **menu** no terminal. Em geral as opções são:
listar (consulta pedida no enunciado), cadastrar os registros de cada tabela e
sair. Por exemplo, no Ex02 e no Ex04 há a criação de pedido/post com seus itens
em **uma transação** (commit só no fim), garantindo consistência.

## Detalhe de cada sistema

- **Ex01 — Livros:** lista livros com autor e editora (JOIN das 3 tabelas);
  cadastra autores, editoras e livros.
- **Ex02 — Pedidos:** lista os pedidos de um cliente com itens e subtotais;
  cria pedido com vários itens (N:N via `ItensPedido`) em transação.
- **Ex03 — Projetos:** lista tarefas de um projeto com responsável e status
  (`LEFT JOIN` mostra até tarefas sem responsável); permite criar tarefa sem
  atribuir (grava `NULL`).
- **Ex04 — Blog:** lista posts de uma categoria com contagem de comentários;
  cria post associando várias categorias (N:N) em transação.
- **Ex05 — Cursos:** lista cursos de um aluno e alunos de um curso; matricula
  (trata duplicidade de matrícula com aviso amigável).

## Notas técnicas

- Todas as queries usam **parâmetros** (`@nome`), nunca concatenação de
  strings — proteção contra SQL Injection.
- Conexões são abertas com `using` (fechadas/devolvidas ao pool ao final).
- O tratamento de erro de duplicidade usa o código MySQL **1062** (chave única).
- O `Db.cs` também concentra a leitura validada de inteiros e decimais do
  console (aceita vírgula ou ponto).
