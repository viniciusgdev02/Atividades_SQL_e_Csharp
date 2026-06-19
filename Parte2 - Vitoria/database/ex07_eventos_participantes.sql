-- Exercício 7: Sistema de Eventos e Participantes
-- Objetivo: gerenciar eventos e os participantes inscritos.
-- Índices: criamos índices em colunas usadas com frequência em filtros/joins
-- para melhorar o desempenho de consultas em tabelas grandes.

DROP DATABASE IF EXISTS eventos;
CREATE DATABASE eventos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE eventos;

-- Tabela: Eventos
CREATE TABLE Eventos (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    nome        VARCHAR(150) NOT NULL,
    data_evento DATE NOT NULL,
    local       VARCHAR(200),
    descricao   TEXT
) ENGINE=InnoDB;

-- Tabela: Participantes
CREATE TABLE Participantes (
    id       INT AUTO_INCREMENT PRIMARY KEY,
    nome     VARCHAR(150) NOT NULL,
    email    VARCHAR(150) NOT NULL UNIQUE,
    telefone VARCHAR(20)
) ENGINE=InnoDB;

-- Tabela: Inscricoes (junção N:N entre Eventos e Participantes)
-- status_pagamento como ENUM para padronizar os valores.
CREATE TABLE Inscricoes (
    evento_id        INT NOT NULL,
    participante_id  INT NOT NULL,
    data_inscricao   DATE NOT NULL DEFAULT (CURRENT_DATE),
    status_pagamento ENUM('pago','pendente','cancelado') NOT NULL DEFAULT 'pendente',
    PRIMARY KEY (evento_id, participante_id),
    CONSTRAINT fk_inscricao_evento      FOREIGN KEY (evento_id)       REFERENCES Eventos(id),
    CONSTRAINT fk_inscricao_participante FOREIGN KEY (participante_id) REFERENCES Participantes(id)
) ENGINE=InnoDB;

-- Índices para acelerar consultas frequentes em bases grandes.
CREATE INDEX idx_eventos_data        ON Eventos(data_evento);
CREATE INDEX idx_inscricoes_status   ON Inscricoes(status_pagamento);

-- DADOS DE EXEMPLO
INSERT INTO Eventos (nome, data_evento, local, descricao) VALUES
    ('Workshop de SQL',    '2026-07-10', 'Auditório A - BH', 'Bancos de dados na prática'),
    ('Meetup .NET',        '2026-07-20', 'Coworking Centro',  'Encontro de devs C#');

INSERT INTO Participantes (nome, email, telefone) VALUES
    ('Ana Souza',  'ana@email.com',  '(31) 9000-0001'),
    ('Bruno Lima', 'bruno@email.com','(31) 9000-0002'),
    ('Carla Dias', 'carla@email.com','(31) 9000-0003');

INSERT INTO Inscricoes (evento_id, participante_id, data_inscricao, status_pagamento) VALUES
    (1, 1, '2026-06-10', 'pago'),
    (1, 2, '2026-06-11', 'pendente'),
    (1, 3, '2026-06-12', 'pago'),
    (2, 1, '2026-06-15', 'pago');

-- CONSULTA SOLICITADA
-- Lista todos os participantes de um evento específico (id = 1) que já
-- pagaram (status_pagamento = 'pago').
SELECT
    p.nome      AS participante,
    p.email,
    i.data_inscricao
FROM Inscricoes i
INNER JOIN Participantes p ON i.participante_id = p.id
WHERE i.evento_id = 1
  AND i.status_pagamento = 'pago'
ORDER BY p.nome;
