INSERT INTO role(name) VALUES ('Student');

CREATE TABLE language_support(
    language_id SERIAL PRIMARY KEY,
    language VARCHAR(25) NOT NULL,
    version VARCHAR(25) NOT NULL
);

INSERT INTO language_support(language, version) VALUES ('haskell', '9.8.2');
INSERT INTO language_support(language, version) VALUES ('python', 'someVersion');

ALTER TABLE solved RENAME TO submission;
ALTER TABLE submission 
    ADD COLUMN solution TEXT NOT NULL DEFAULT '',
    ADD COLUMN language_id INTEGER REFERENCES language_support(language_id) NOT NULL DEFAULT 9999999,
    ADD COLUMN solved BOOLEAN NOT NULL DEFAULT false;

CREATE TABLE language_in_session (
    session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE,
    language_id INTEGER REFERENCES language_support(language_id) ON DELETE CASCADE,
    PRIMARY KEY (session_id, language_id)
);

CREATE TABLE user_in_timedsession (
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, session_id)
);

DROP TABLE anon_users;
DROP TABLE app_users;

ALTER TABLE users 
    ADD COLUMN email VARCHAR(50) UNIQUE,
    ADD COLUMN name VARCHAR(100) NOT NULL DEFAULT '',
    ADD COLUMN password_hash VARCHAR(255),
    ADD COLUMN anonymous BOOLEAN DEFAULT True;

