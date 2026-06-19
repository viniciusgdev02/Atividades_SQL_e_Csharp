-- Exercício 10: Sistema de Gerenciamento de Restaurantes
-- Objetivo: organizar pratos, ingredientes e suas receitas.
-- A tabela Receitas guarda QUANTO de cada ingrediente um prato usa.

DROP DATABASE IF EXISTS restaurante;
CREATE DATABASE restaurante CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE restaurante;

-- Tabela: Pratos
CREATE TABLE Pratos (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    nome      VARCHAR(150) NOT NULL,
    descricao TEXT,
    preco     DECIMAL(10,2) NOT NULL
) ENGINE=InnoDB;

-- Tabela: Ingredientes
-- unidade_medida indica como o ingrediente é medido (g, ml, unidade...).
CREATE TABLE Ingredientes (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    nome           VARCHAR(150) NOT NULL,
    unidade_medida VARCHAR(20) NOT NULL
) ENGINE=InnoDB;

-- Tabela: Receitas (junção N:N entre Pratos e Ingredientes)
-- Chave primária composta (prato_id, ingrediente_id): cada ingrediente
-- aparece uma única vez por prato.
-- quantidade_necessaria é DECIMAL para aceitar valores fracionados (ex.: 0.5).
CREATE TABLE Receitas (
    prato_id              INT NOT NULL,
    ingrediente_id        INT NOT NULL,
    quantidade_necessaria DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (prato_id, ingrediente_id),
    CONSTRAINT fk_receita_prato       FOREIGN KEY (prato_id)       REFERENCES Pratos(id),
    CONSTRAINT fk_receita_ingrediente FOREIGN KEY (ingrediente_id) REFERENCES Ingredientes(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Pratos (nome, descricao, preco) VALUES
    ('Macarrão à Bolonhesa', 'Massa com molho de carne', 32.00),
    ('Omelete',              'Omelete simples',          18.00);

INSERT INTO Ingredientes (nome, unidade_medida) VALUES
    ('Macarrão',   'g'),
    ('Carne moída','g'),
    ('Molho de tomate','ml'),
    ('Ovo',        'unidade'),
    ('Sal',        'g');

-- Receita do Macarrão à Bolonhesa (prato_id = 1)
INSERT INTO Receitas (prato_id, ingrediente_id, quantidade_necessaria) VALUES
    (1, 1, 200.00),  -- 200g de macarrão
    (1, 2, 150.00),  -- 150g de carne moída
    (1, 3, 100.00),  -- 100ml de molho
    (1, 5,   5.00),  -- 5g de sal
-- Receita do Omelete (prato_id = 2)
    (2, 4,   3.00),  -- 3 ovos
    (2, 5,   2.00);  -- 2g de sal

-- CONSULTA SOLICITADA
-- Lista todos os ingredientes necessários para um prato específico (id = 1),
-- incluindo a quantidade e a unidade de medida.
SELECT
    p.nome                       AS prato,
    i.nome                       AS ingrediente,
    r.quantidade_necessaria,
    i.unidade_medida
FROM Receitas r
INNER JOIN Pratos       p ON r.prato_id       = p.id
INNER JOIN Ingredientes i ON r.ingrediente_id = i.id
WHERE r.prato_id = 1
ORDER BY i.nome;
