-- Exercício 2: Registro de Pedidos Online
-- Objetivo: rastrear clientes, produtos e os pedidos de uma loja virtual.
-- ItensPedido é a tabela de junção (relacionamento muitos-para-muitos):
-- cada linha representa "X unidades do produto P dentro do pedido D".

DROP DATABASE IF EXISTS loja_online;
CREATE DATABASE loja_online CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE loja_online;

-- Tabela: Clientes
CREATE TABLE Clientes (
    id       INT AUTO_INCREMENT PRIMARY KEY,
    nome     VARCHAR(150) NOT NULL,
    email    VARCHAR(150) NOT NULL UNIQUE,   -- email não pode se repetir
    endereco VARCHAR(255)
) ENGINE=InnoDB;

-- Tabela: Produtos
CREATE TABLE Produtos (
    id      INT AUTO_INCREMENT PRIMARY KEY,
    nome    VARCHAR(150) NOT NULL,
    preco   DECIMAL(10,2) NOT NULL,          -- valores monetários: DECIMAL
    estoque INT NOT NULL DEFAULT 0
) ENGINE=InnoDB;

-- Tabela: Pedidos
-- Cada pedido pertence a um cliente.
CREATE TABLE Pedidos (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id  INT NOT NULL,
    data_pedido DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status      VARCHAR(30) NOT NULL DEFAULT 'pendente',
    CONSTRAINT fk_pedido_cliente FOREIGN KEY (cliente_id) REFERENCES Clientes(id)
) ENGINE=InnoDB;

-- Tabela: ItensPedido (junção N:N entre Pedidos e Produtos)
-- A chave primária composta (pedido_id, produto_id) impede que o mesmo
-- produto apareça duas vezes no mesmo pedido.
-- preco_unitario guarda o preço no momento da compra (histórico).
CREATE TABLE ItensPedido (
    pedido_id      INT NOT NULL,
    produto_id     INT NOT NULL,
    quantidade     INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (pedido_id, produto_id),
    CONSTRAINT fk_item_pedido  FOREIGN KEY (pedido_id)  REFERENCES Pedidos(id),
    CONSTRAINT fk_item_produto FOREIGN KEY (produto_id) REFERENCES Produtos(id)
) ENGINE=InnoDB;

-- DADOS DE EXEMPLO
INSERT INTO Clientes (nome, email, endereco) VALUES
    ('Ana Souza',    'ana@email.com',    'Rua A, 100 - BH'),
    ('Bruno Lima',   'bruno@email.com',  'Av. B, 200 - BH'),
    ('Carla Dias',   'carla@email.com',  'Rua C, 300 - Betim');

INSERT INTO Produtos (nome, preco, estoque) VALUES
    ('Teclado Mecânico', 250.00, 30),
    ('Mouse Gamer',      120.00, 50),
    ('Monitor 24"',      900.00, 15),
    ('Headset',          180.00, 40);

INSERT INTO Pedidos (cliente_id, data_pedido, status) VALUES
    (1, '2026-06-01 10:00:00', 'pago'),
    (1, '2026-06-10 14:30:00', 'pendente'),
    (2, '2026-06-12 09:15:00', 'pago');

-- Itens do pedido 1 (cliente Ana): teclado + mouse
INSERT INTO ItensPedido (pedido_id, produto_id, quantidade, preco_unitario) VALUES
    (1, 1, 1, 250.00),
    (1, 2, 2, 120.00),
-- Itens do pedido 2 (cliente Ana): monitor
    (2, 3, 1, 900.00),
-- Itens do pedido 3 (cliente Bruno): headset
    (3, 4, 1, 180.00);

-- CONSULTA SOLICITADA
-- Todos os pedidos de um cliente específico (id = 1), com produtos,
-- quantidades e o subtotal de cada item.
SELECT
    p.id                              AS pedido,
    p.data_pedido,
    p.status,
    pr.nome                           AS produto,
    ip.quantidade,
    ip.preco_unitario,
    (ip.quantidade * ip.preco_unitario) AS subtotal
FROM Pedidos p
INNER JOIN ItensPedido ip ON p.id          = ip.pedido_id
INNER JOIN Produtos    pr ON ip.produto_id = pr.id
WHERE p.cliente_id = 1
ORDER BY p.id;
