-- Exercício 3: Gerenciamento de Projetos e Tarefas
-- Objetivo: organizar projetos e suas tarefas, sabendo quem trabalha em quê
--           e o status de cada tarefa.

DROP DATABASE IF EXISTS gestao_projetos;
CREATE DATABASE gestao_projetos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE gestao_projetos;

-- Tabela: Usuarios
CREATE TABLE Usuarios (
    id    INT AUTO_INCREMENT PRIMARY KEY,
    nome  VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabela: Projetos
-- Usa o tipo DATE para datas (sem hora).
CREATE TABLE Projetos (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    nome        VARCHAR(150) NOT NULL,
    descricao   TEXT,
    data_inicio DATE,
    data_fim    DATE
) ENGINE=InnoDB;

-- Tabela: Tarefas
-- Cada tarefa pertence a um projeto e é atribuída a um usuário.
-- usuario_id é NULL-able: uma tarefa pode existir antes de ser atribuída.
CREATE TABLE Tarefas (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    projeto_id      INT NOT NULL,
    usuario_id      INT,
    descricao       VARCHAR(255) NOT NULL,
    status          VARCHAR(30) NOT NULL DEFAULT 'a fazer',
    data_vencimento DATE,
    CONSTRAINT fk_tarefa_projeto FOREIGN KEY (projeto_id) REFERENCES Projetos(id),
    CONSTRAINT fk_tarefa_usuario FOREIGN KEY (usuario_id) REFERENCES Usuarios(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Usuarios (nome, email) VALUES
    ('Vinicius Costa', 'vinicius@empresa.com'),
    ('Mariana Reis',   'mariana@empresa.com'),
    ('Pedro Alves',    'pedro@empresa.com');

INSERT INTO Projetos (nome, descricao, data_inicio, data_fim) VALUES
    ('Sistema de Estoque', 'App interno de controle de estoque', '2026-05-01', '2026-08-30'),
    ('Site Institucional', 'Novo site da empresa',               '2026-06-01', '2026-07-15');

INSERT INTO Tarefas (projeto_id, usuario_id, descricao, status, data_vencimento) VALUES
    (1, 1, 'Modelar o banco de dados',     'concluída',  '2026-05-10'),
    (1, 2, 'Desenvolver tela de entrada',  'em andamento','2026-06-20'),
    (1, 3, 'Escrever testes',              'a fazer',    '2026-07-01'),
    (2, 2, 'Criar layout da home',         'em andamento','2026-06-25'),
    (2, NULL, 'Revisar textos',            'a fazer',    '2026-07-05'); -- ainda sem responsável

-- CONSULTA SOLICITADA
-- Lista todas as tarefas de um projeto específico (id = 1), mostrando o nome
-- do usuário atribuído e o status. Usa LEFT JOIN para também exibir tarefas
-- que ainda não têm responsável.
SELECT
    t.descricao                      AS tarefa,
    COALESCE(u.nome, '(não atribuída)') AS responsavel,
    t.status,
    t.data_vencimento
FROM Tarefas t
LEFT JOIN Usuarios u ON t.usuario_id = u.id
WHERE t.projeto_id = 1
ORDER BY t.data_vencimento;
