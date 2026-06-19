-- Exercício 6: Sistema de Inventário de Loja
-- Objetivo: controlar produtos, fornecedores e movimentações de estoque.
-- O estoque atual NÃO é guardado fixo: é calculado somando entradas e
-- subtraindo saídas registradas em TransacoesEstoque.

DROP DATABASE IF EXISTS inventario_loja;
CREATE DATABASE inventario_loja CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE inventario_loja;

-- Tabela: Fornecedores
CREATE TABLE Fornecedores (
    id       INT AUTO_INCREMENT PRIMARY KEY,
    nome     VARCHAR(150) NOT NULL,
    contato  VARCHAR(150),
    telefone VARCHAR(20)
) ENGINE=InnoDB;

-- Tabela: Produtos
-- Cada produto tem um fornecedor.
CREATE TABLE Produtos (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nome          VARCHAR(150) NOT NULL,
    descricao     TEXT,
    preco_compra  DECIMAL(10,2) NOT NULL,
    preco_venda   DECIMAL(10,2) NOT NULL,
    fornecedor_id INT NOT NULL,
    CONSTRAINT fk_produto_fornecedor FOREIGN KEY (fornecedor_id)
        REFERENCES Fornecedores(id)
) ENGINE=InnoDB;

-- Tabela: TransacoesEstoque
-- tipo_transacao usa ENUM para limitar os valores a 'entrada' ou 'saida'.
CREATE TABLE TransacoesEstoque (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    produto_id      INT NOT NULL,
    tipo_transacao  ENUM('entrada','saida') NOT NULL,
    quantidade      INT NOT NULL,
    data_transacao  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_transacao_produto FOREIGN KEY (produto_id)
        REFERENCES Produtos(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Fornecedores (nome, contato, telefone) VALUES
    ('TechDistribuidora', 'Roberto',  '(31) 9999-0001'),
    ('AcessóriosBR',      'Fernanda', '(31) 9999-0002');

INSERT INTO Produtos (nome, descricao, preco_compra, preco_venda, fornecedor_id) VALUES
    ('Teclado',  'Teclado USB',  80.00, 150.00, 1),
    ('Mouse',    'Mouse óptico', 30.00,  70.00, 1),
    ('Cabo HDMI','Cabo 2m',      10.00,  25.00, 2);

-- Movimentações: entradas (compras) e saídas (vendas)
INSERT INTO TransacoesEstoque (produto_id, tipo_transacao, quantidade, data_transacao) VALUES
    (1, 'entrada', 100, '2026-06-01 10:00:00'),
    (1, 'saida',    30, '2026-06-05 15:00:00'),
    (2, 'entrada',  50, '2026-06-01 10:00:00'),
    (2, 'saida',    20, '2026-06-06 11:00:00'),
    (2, 'saida',     5, '2026-06-09 09:00:00'),
    (3, 'entrada',  80, '2026-06-02 12:00:00');

-- CONSULTA SOLICITADA
-- Calcula o estoque atual de cada produto:
--   soma das entradas - soma das saídas.
-- Usa CASE dentro de SUM para diferenciar entrada de saída.
SELECT
    p.nome AS produto,
    COALESCE(SUM(CASE WHEN t.tipo_transacao = 'entrada' THEN t.quantidade ELSE 0 END), 0)
      - COALESCE(SUM(CASE WHEN t.tipo_transacao = 'saida' THEN t.quantidade ELSE 0 END), 0)
      AS estoque_atual
FROM Produtos p
LEFT JOIN TransacoesEstoque t ON t.produto_id = p.id
GROUP BY p.id, p.nome
ORDER BY p.nome;
