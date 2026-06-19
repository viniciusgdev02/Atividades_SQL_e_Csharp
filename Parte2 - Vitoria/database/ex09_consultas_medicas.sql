-- Exercício 9: Gerenciamento de Consultas Médicas
-- Objetivo: agendar consultas relacionando pacientes e médicos.
-- data_hora usa o tipo DATETIME (data + hora da consulta).

DROP DATABASE IF EXISTS clinica;
CREATE DATABASE clinica CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE clinica;

-- Tabela: Pacientes
CREATE TABLE Pacientes (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    nome             VARCHAR(150) NOT NULL,
    data_nascimento  DATE,
    telefone         VARCHAR(20),
    endereco         VARCHAR(255)
) ENGINE=InnoDB;

-- Tabela: Medicos
-- CRM é único: identifica o médico profissionalmente.
CREATE TABLE Medicos (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nome          VARCHAR(150) NOT NULL,
    especialidade VARCHAR(100),
    CRM           VARCHAR(20) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabela: Consultas
-- Relaciona um paciente e um médico em uma data/hora.
CREATE TABLE Consultas (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    paciente_id  INT NOT NULL,
    medico_id    INT NOT NULL,
    data_hora    DATETIME NOT NULL,
    status       VARCHAR(30) NOT NULL DEFAULT 'agendada',
    observacoes  TEXT,
    CONSTRAINT fk_consulta_paciente FOREIGN KEY (paciente_id) REFERENCES Pacientes(id),
    CONSTRAINT fk_consulta_medico   FOREIGN KEY (medico_id)   REFERENCES Medicos(id)
) ENGINE=InnoDB;

-- Índice para acelerar a busca por agenda de um médico em uma data.
CREATE INDEX idx_consulta_medico_data ON Consultas(medico_id, data_hora);

-- DADOS DE EXEMPLO
INSERT INTO Pacientes (nome, data_nascimento, telefone, endereco) VALUES
    ('João Pereira', '1990-04-12', '(31) 8000-0001', 'Rua X, 10 - BH'),
    ('Maria Castro', '1985-09-23', '(31) 8000-0002', 'Av. Y, 50 - Betim');

INSERT INTO Medicos (nome, especialidade, CRM) VALUES
    ('Dra. Helena Reis', 'Cardiologia',  'CRM-MG 12345'),
    ('Dr. Paulo Matos',  'Clínico Geral','CRM-MG 67890');

INSERT INTO Consultas (paciente_id, medico_id, data_hora, status, observacoes) VALUES
    (1, 1, '2026-07-01 09:00:00', 'agendada', 'Primeira consulta'),
    (2, 1, '2026-07-01 10:00:00', 'agendada', 'Retorno'),
    (1, 2, '2026-07-02 14:00:00', 'agendada', NULL),
    (2, 1, '2026-07-05 08:30:00', 'agendada', 'Exames');

-- CONSULTA SOLICITADA
-- Lista todas as consultas agendadas para um médico específico (id = 1)
-- em uma data determinada (2026-07-01).
-- DATE(data_hora) extrai só a parte da data para o filtro.
SELECT
    c.data_hora,
    p.nome       AS paciente,
    c.status,
    c.observacoes
FROM Consultas c
INNER JOIN Pacientes p ON c.paciente_id = p.id
WHERE c.medico_id = 1
  AND DATE(c.data_hora) = '2026-07-01'
ORDER BY c.data_hora;
