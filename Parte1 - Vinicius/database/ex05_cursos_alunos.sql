-- Exercício 5: Gerenciamento de Cursos e Alunos
-- Objetivo: gerenciar cursos online e os alunos matriculados.
-- Integridade referencial: as chaves estrangeiras em Matriculas garantem que
-- só é possível matricular um aluno que existe em um curso que existe.

DROP DATABASE IF EXISTS plataforma_cursos;
CREATE DATABASE plataforma_cursos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE plataforma_cursos;

-- Tabela: Alunos
CREATE TABLE Alunos (
    id    INT AUTO_INCREMENT PRIMARY KEY,
    nome  VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabela: Cursos
CREATE TABLE Cursos (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    titulo    VARCHAR(150) NOT NULL,
    descricao TEXT,
    instrutor VARCHAR(150)
) ENGINE=InnoDB;

-- Tabela: Matriculas (tabela de junção N:N)
-- Chave primária composta (aluno_id, curso_id) impede que o mesmo aluno
-- seja matriculado duas vezes no mesmo curso.
CREATE TABLE Matriculas (
    aluno_id        INT NOT NULL,
    curso_id        INT NOT NULL,
    data_matricula  DATE NOT NULL DEFAULT (CURRENT_DATE),
    status          VARCHAR(30) NOT NULL DEFAULT 'ativa',
    PRIMARY KEY (aluno_id, curso_id),
    CONSTRAINT fk_matricula_aluno FOREIGN KEY (aluno_id) REFERENCES Alunos(id),
    CONSTRAINT fk_matricula_curso FOREIGN KEY (curso_id) REFERENCES Cursos(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Alunos (nome, email) VALUES
    ('Vinicius Costa', 'vinicius@aluno.com'),
    ('Beatriz Nunes',  'beatriz@aluno.com'),
    ('Diego Ramos',    'diego@aluno.com');

INSERT INTO Cursos (titulo, descricao, instrutor) VALUES
    ('SQL do Zero',         'Bancos de dados relacionais na prática.', 'Prof. Silva'),
    ('C# .NET',             'Desenvolvimento de aplicações desktop.',  'Prof. Souza'),
    ('Python Automação',    'Automatizando tarefas com Python.',       'Prof. Lima');

INSERT INTO Matriculas (aluno_id, curso_id, data_matricula, status) VALUES
    (1, 1, '2026-06-01', 'ativa'),
    (1, 2, '2026-06-02', 'ativa'),
    (1, 3, '2026-06-03', 'concluída'),
    (2, 1, '2026-06-05', 'ativa'),
    (3, 2, '2026-06-06', 'ativa');

-- CONSULTAS SOLICITADAS

-- (A) Todos os cursos em que um aluno específico (id = 1) está matriculado.
SELECT
    c.titulo      AS curso,
    c.instrutor,
    m.data_matricula,
    m.status
FROM Matriculas m
INNER JOIN Cursos c ON m.curso_id = c.id
WHERE m.aluno_id = 1
ORDER BY m.data_matricula;

-- (B) Todos os alunos matriculados em um curso específico (id = 1).
SELECT
    a.nome        AS aluno,
    a.email,
    m.data_matricula,
    m.status
FROM Matriculas m
INNER JOIN Alunos a ON m.aluno_id = a.id
WHERE m.curso_id = 1
ORDER BY a.nome;
