# Trabalho de Banco de Dados - Sistemas em C# + MySQL

Trabalho em dupla com os 10 exercícios de modelagem e acesso a banco de dados.
Cada exercício é um **sistema de console em C# (.NET 8)** que cria e consome um
banco **MySQL** próprio, usando **ADO.NET puro (MySqlConnector)**.

Os exercícios foram divididos em duas partes (5 para cada integrante):

- **Parte 1** - Exercícios 1 a 5
- **Parte 2** - Exercícios 6 a 10

> Integrantes: ____________________ (Parte 1) e ____________________ (Parte 2)

## Exercícios

| # | Sistema | Banco | Parte |
|---|---|---|---|
| 1 | Gerenciamento de Livros | `biblioteca` | 1 |
| 2 | Pedidos Online | `loja_online` | 1 |
| 3 | Projetos e Tarefas | `gestao_projetos` | 1 |
| 4 | Blog Simples | `blog` | 1 |
| 5 | Cursos e Alunos | `plataforma_cursos` | 1 |
| 6 | Inventário de Loja | `inventario_loja` | 2 |
| 7 | Eventos e Participantes | `eventos` | 2 |
| 8 | Avaliação de Filmes | `avaliacao_filmes` | 2 |
| 9 | Consultas Médicas | `clinica` | 2 |
| 10 | Gerenciamento de Restaurantes | `restaurante` | 2 |

## Stack

- C# / .NET 8 (aplicativos de console)
- MySqlConnector (driver ADO.NET, sem ORM)
- MySQL 8.0+ ou MariaDB 10.4+

## Estrutura geral

```
.
├── README.md            (este arquivo)
├── Parte1/              Exercícios 1 a 5
│   ├── Parte1.sln
│   ├── README.md        instruções detalhadas da Parte 1
│   ├── database/        scripts SQL que criam os bancos
│   └── ExNN_.../        um projeto de console por exercício
└── Parte2/              Exercícios 6 a 10
    ├── Parte2.sln
    ├── README.md        instruções detalhadas da Parte 2
    ├── database/
    └── ExNN_.../
```

Cada projeto tem o mesmo formato: `Db.cs` (conexão + leitura do console) e
`Program.cs` (menu e operações).

## Como rodar (resumo)

Os passos completos estão nos READMEs de cada parte. Em resumo:

1. **Criar o banco** - rodar o script SQL correspondente em `ParteX/database/`:
   ```bash
   mysql -u root -p < ex01_gerenciamento_livros.sql
   ```
2. **Configurar a conexão** - ajustar usuário/senha em `Db.cs` ou definir a
   variável de ambiente `MYSQL_CONN`.
3. **Executar o exercício** - dentro da pasta da parte:
   ```bash
   dotnet restore Parte1.sln
   dotnet run --project Ex01_GerenciamentoLivros
   ```

O programa abre um menu no terminal com a consulta pedida no enunciado e as
opções de cadastro de cada tabela.

## O que cada sistema faz

Todos seguem a mesma ideia: a consulta exigida pelo exercício mais o cadastro
dos registros das tabelas envolvidas. Casos que valem destacar:

- **Pedidos (Ex2), Blog (Ex4) e Restaurante (Ex10):** criam um registro "pai" e
  seus filhos (itens / categorias / receita) dentro de uma transação - ou grava
  tudo ou nada.
- **Inventário (Ex6):** o estoque não fica guardado numa coluna; é calculado a
  partir das movimentações (entradas menos saídas).
- **Cursos (Ex5), Eventos (Ex7), Filmes (Ex8) e Consultas (Ex9):** tratam
  tentativas de registro duplicado (matrícula, inscrição, avaliação, CRM) com
  uma mensagem amigável em vez de quebrar.

## Observações

- Todas as queries usam parâmetros (`@nome`) em vez de concatenar strings.
- As conexões são abertas com `using` e devolvidas ao pool ao final.
- Os scripts SQL recriam o banco do zero (`DROP DATABASE IF EXISTS`), então
  podem ser rodados de novo sem dar erro.

Para os detalhes de cada metade, ver `Parte1/README.md` e `Parte2/README.md`.
