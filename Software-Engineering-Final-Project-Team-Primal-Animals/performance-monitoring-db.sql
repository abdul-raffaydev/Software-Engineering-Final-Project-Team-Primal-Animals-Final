CREATE SCHEMA `performance-monitoring-db`;
use `performance-monitoring-db`;

-- dated 16th November 2025
-- contains user details
CREATE TABLE users (
    user_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_name VARCHAR(150),
    email VARCHAR(150),
    password VARCHAR(500),
    insertion_timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);


-- contains user roles e.g patient, clinician, admin
CREATE TABLE roles (
    role_id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE

-- to assign different roles to one user
CREATE TABLE user_role_map (
    user_id BIGINT NOT NULL,
    role_id INT NOT NULL,
    
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE
););

-- to avoid duplicate combinations of user_id and role_id and that not one user_id can be re-assigned same role_id twice
ALTER TABLE user_role_map
ADD UNIQUE (user_id, role_id);

-- a user can be all or one of these three types
INSERT INTO roles (role_name)
VALUES ('PATIENT'), ('CLINICIAN'), ('ADMIN');


-- patient information 
CREATE TABLE patient_info (
    user_id BIGINT PRIMARY KEY,
    patient_name VARCHAR(150) NOT NULL,
    status VARCHAR(50) NOT NULL,
    last_activity TIMESTAMP NULL DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

CREATE TABLE patient_info (
    user_id BIGINT PRIMARY KEY,
    patient_name VARCHAR(150) NOT NULL,
    status VARCHAR(50) NOT NULL,
    last_activity TIMESTAMP NULL DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- stores 32 by 32 byte array
CREATE TABLE pressure_frame (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT NOT NULL,
    recorded_at TIMESTAMP NOT NULL,
    frame_data BLOB NOT NULL,
    peak_pressure_index INT,
    contact_area_percent DOUBLE,
    alert_flag BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    INDEX idx_user_time (user_id, recorded_at)
);

-- each frame comment refers to a pressure_frame in db
CREATE TABLE frame_comments (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    frame_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    comments TEXT NOT NULL,
    insertion_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (frame_id) REFERENCES pressure_frame(id),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

CREATE TABLE generated_report (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT NOT NULL,
    period VARCHAR(50),
    start_at TIMESTAMP,
    end_at TIMESTAMP,
    report_file_path VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
