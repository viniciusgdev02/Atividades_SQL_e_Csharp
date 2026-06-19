-- Exercício 4: Sistema de Blog Simples
-- Objetivo: organizar posts, categorias e comentários de um blog.

DROP DATABASE IF EXISTS blog;
CREATE DATABASE blog CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE blog;

-- Tabela: Posts
CREATE TABLE Posts (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    titulo           VARCHAR(200) NOT NULL,
    conteudo         TEXT,
    data_publicacao  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- Tabela: Categorias
CREATE TABLE Categorias (
    id   INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabela: Comentarios
-- Cada comentário pertence a um post. ON DELETE CASCADE: se o post for
-- apagado, seus comentários somem junto.
CREATE TABLE Comentarios (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    post_id          INT NOT NULL,
    autor            VARCHAR(150) NOT NULL,
    conteudo         TEXT,
    data_comentario  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_comentario_post FOREIGN KEY (post_id)
        REFERENCES Posts(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Tabela: PostCategorias (junção N:N entre Posts e Categorias)
-- Chave primária composta evita associar a mesma categoria ao mesmo post 2x.
CREATE TABLE PostCategorias (
    post_id      INT NOT NULL,
    categoria_id INT NOT NULL,
    PRIMARY KEY (post_id, categoria_id),
    CONSTRAINT fk_pc_post      FOREIGN KEY (post_id)      REFERENCES Posts(id) ON DELETE CASCADE,
    CONSTRAINT fk_pc_categoria FOREIGN KEY (categoria_id) REFERENCES Categorias(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Posts (titulo, conteudo, data_publicacao) VALUES
    ('Introdução ao SQL',  'Conceitos básicos de bancos relacionais.', '2026-06-01 09:00:00'),
    ('Dicas de Carreira',  'Como montar um portfólio de dev.',          '2026-06-05 11:00:00'),
    ('Receita de Café',    'Um bom café para programar.',               '2026-06-08 08:00:00');

INSERT INTO Categorias (nome) VALUES
    ('Tecnologia'),
    ('Carreira'),
    ('Lifestyle');

-- Um post pode ter várias categorias:
INSERT INTO PostCategorias (post_id, categoria_id) VALUES
    (1, 1),          -- "Introdução ao SQL"  -> Tecnologia
    (2, 1), (2, 2),  -- "Dicas de Carreira"  -> Tecnologia + Carreira
    (3, 3);          -- "Receita de Café"    -> Lifestyle

INSERT INTO Comentarios (post_id, autor, conteudo) VALUES
    (1, 'João',   'Ótimo post!'),
    (1, 'Maria',  'Ajudou bastante.'),
    (2, 'Carlos', 'Dicas valiosas.'),
    (1, 'Lúcia',  'Aguardo a parte 2.');

-- CONSULTA SOLICITADA
-- Lista todos os posts de uma categoria específica (Tecnologia, id = 1),
-- incluindo o número de comentários de cada post.
-- LEFT JOIN garante que posts sem comentários apareçam com contagem 0.
SELECT
    p.titulo                 AS post,
    p.data_publicacao,
    COUNT(c.id)              AS total_comentarios
FROM PostCategorias pc
INNER JOIN Posts       p ON pc.post_id = p.id
LEFT  JOIN Comentarios c ON c.post_id  = p.id
WHERE pc.categoria_id = 1
GROUP BY p.id, p.titulo, p.data_publicacao
ORDER BY total_comentarios DESC;
