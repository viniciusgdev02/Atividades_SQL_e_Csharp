-- Exercício 8: Sistema de Avaliação de Filmes
-- Objetivo: permitir que usuários avaliem filmes e deixem resenhas.
-- A nota é DECIMAL(2,1) para permitir notas como 8.5 (escala de 0 a 10).

DROP DATABASE IF EXISTS avaliacao_filmes;
CREATE DATABASE avaliacao_filmes CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE avaliacao_filmes;

-- Tabela: Usuarios
CREATE TABLE Usuarios (
    id    INT AUTO_INCREMENT PRIMARY KEY,
    nome  VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabela: Filmes
CREATE TABLE Filmes (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    titulo         VARCHAR(200) NOT NULL,
    ano_lancamento INT,
    diretor        VARCHAR(150),
    genero         VARCHAR(80)
) ENGINE=InnoDB;

-- Tabela: Avaliacoes (junção N:N entre Usuarios e Filmes)
-- A restrição UNIQUE (usuario_id, filme_id) impede que o mesmo usuário
-- avalie o mesmo filme mais de uma vez.
CREATE TABLE Avaliacoes (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id      INT NOT NULL,
    filme_id        INT NOT NULL,
    nota            DECIMAL(3,1) NOT NULL,   -- ex.: 8.5 ou 10.0 (escala 0 a 10)
    comentario      TEXT,
    data_avaliacao  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_avaliacao_usuario FOREIGN KEY (usuario_id) REFERENCES Usuarios(id),
    CONSTRAINT fk_avaliacao_filme   FOREIGN KEY (filme_id)   REFERENCES Filmes(id),
    CONSTRAINT uq_usuario_filme UNIQUE (usuario_id, filme_id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Usuarios (nome, email) VALUES
    ('Ana Souza',  'ana@email.com'),
    ('Bruno Lima', 'bruno@email.com'),
    ('Carla Dias', 'carla@email.com');

INSERT INTO Filmes (titulo, ano_lancamento, diretor, genero) VALUES
    ('A Origem',          2010, 'Christopher Nolan', 'Ficção'),
    ('Cidade de Deus',    2002, 'Fernando Meirelles','Drama'),
    ('Interestelar',      2014, 'Christopher Nolan', 'Ficção');

INSERT INTO Avaliacoes (usuario_id, filme_id, nota, comentario) VALUES
    (1, 1, 9.0, 'Roteiro genial.'),
    (2, 1, 8.0, 'Confuso mas bom.'),
    (3, 1, 9.5, 'Meu favorito.'),
    (1, 2, 10.0,'Obra-prima nacional.'),
    (2, 3, 8.5, 'Ótima trilha sonora.');

-- CONSULTA SOLICITADA

-- (A) Média de avaliação de um filme específico (id = 1).
SELECT
    f.titulo,
    ROUND(AVG(a.nota), 2) AS media_nota,
    COUNT(a.id)           AS total_avaliacoes
FROM Filmes f
LEFT JOIN Avaliacoes a ON a.filme_id = f.id
WHERE f.id = 1
GROUP BY f.id, f.titulo;

-- (B) Todos os comentários desse mesmo filme.
SELECT
    u.nome       AS usuario,
    a.nota,
    a.comentario,
    a.data_avaliacao
FROM Avaliacoes a
INNER JOIN Usuarios u ON a.usuario_id = u.id
WHERE a.filme_id = 1
ORDER BY a.data_avaliacao DESC;
