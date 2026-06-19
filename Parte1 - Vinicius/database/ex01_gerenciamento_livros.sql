-- Exercício 1: Sistema de Gerenciamento de Livros
-- Objetivo: catalogar livros de uma biblioteca, relacionando-os a seus
--           autores e editoras.

-- Recria o banco do zero para que o script possa ser executado várias vezes.
DROP DATABASE IF EXISTS biblioteca;
CREATE DATABASE biblioteca CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE biblioteca;

-- Tabela: Autores
-- Lado "1" do relacionamento com Livros.
CREATE TABLE Autores (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(150) NOT NULL,
    nacionalidade VARCHAR(80)
) ENGINE=InnoDB;

-- Tabela: Editoras
-- Lado "1" do relacionamento com Livros.
CREATE TABLE Editoras (
    id     INT AUTO_INCREMENT PRIMARY KEY,
    nome   VARCHAR(150) NOT NULL,
    cidade VARCHAR(100)
) ENGINE=InnoDB;

-- Tabela: Livros
-- Lado "N": cada livro aponta para 1 autor e 1 editora via chave estrangeira.
CREATE TABLE Livros (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    titulo          VARCHAR(200) NOT NULL,
    ano_publicacao  INT,
    autor_id        INT NOT NULL,
    editora_id      INT NOT NULL,
    -- Garante que todo livro pertence a um autor e a uma editora válidos.
    CONSTRAINT fk_livro_autor   FOREIGN KEY (autor_id)   REFERENCES Autores(id),
    CONSTRAINT fk_livro_editora FOREIGN KEY (editora_id) REFERENCES Editoras(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO

-- 5 autores
INSERT INTO Autores (nome, nacionalidade) VALUES
    ('Machado de Assis',        'Brasileira'),
    ('Clarice Lispector',       'Brasileira'),
    ('Jorge Amado',             'Brasileira'),
    ('Gabriel García Márquez',  'Colombiana'),
    ('George Orwell',           'Britânica');

-- 3 editoras
INSERT INTO Editoras (nome, cidade) VALUES
    ('Companhia das Letras', 'São Paulo'),
    ('Record',               'Rio de Janeiro'),
    ('Penguin',              'Londres');

-- 10 livros (autor_id / editora_id referenciam os IDs gerados acima)
INSERT INTO Livros (titulo, ano_publicacao, autor_id, editora_id) VALUES
    ('Dom Casmurro',                1899, 1, 1),
    ('Memórias Póstumas de Brás Cubas', 1881, 1, 2),
    ('A Hora da Estrela',           1977, 2, 1),
    ('A Paixão Segundo G.H.',       1964, 2, 1),
    ('Capitães da Areia',           1937, 3, 2),
    ('Gabriela, Cravo e Canela',    1958, 3, 2),
    ('Cem Anos de Solidão',         1967, 4, 1),
    ('O Amor nos Tempos do Cólera', 1985, 4, 3),
    ('1984',                        1949, 5, 3),
    ('A Revolução dos Bichos',      1945, 5, 3);

-- CONSULTA SOLICITADA
-- Lista todos os livros com seus respectivos autores e editoras.
SELECT
    l.titulo                AS livro,
    l.ano_publicacao        AS ano,
    a.nome                  AS autor,
    e.nome                  AS editora
FROM Livros l
INNER JOIN Autores  a ON l.autor_id   = a.id
INNER JOIN Editoras e ON l.editora_id = e.id
ORDER BY a.nome, l.titulo;
