INSERT INTO role(name) VALUES ('Student');

CREATE TABLE classroom (
	classroom_id SERIAL PRIMARY KEY,
	title VARCHAR(100),
    description VARCHAR(1000),
	owner INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
	roomcode VARCHAR(6) NOT NULL,
	registration_open BOOLEAN NOT NULL
);

CREATE TABLE session_in_classroom(
	classroom_id INTEGER REFERENCES classroom(classroom_id) ON DELETE CASCADE NOT NULL,
	session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE NOT NULL,
	active BOOLEAN NOT NULL,
	PRIMARY KEY(classroom_id, session_id)
);

CREATE TABLE student_in_classroom(
	student_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
	classroom_id INTEGER REFERENCES classroom(classroom_id) ON DELETE CASCADE NOT NULL,
	PRIMARY KEY(student_id, classroom_id)
);

CREATE TABLE language_support(
    language_id SERIAL PRIMARY KEY,
    language VARCHAR(25) NOT NULL,
    version VARCHAR(25) NOT NULL
);

INSERT INTO language_support(language, version) VALUES ('Haskell', '9.8.2');
INSERT INTO language_support(language, version) VALUES ('Python', 'someVersion');

ALTER TABLE solved RENAME TO submission;
ALTER TABLE submission 
    ADD COLUMN solution TEXT,
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

ALTER TABLE session
    ALTER COLUMN expirationtime_utc DROP NOT NULL,
    ALTER COLUMN session_code DROP NOT NULL;


DROP TRIGGER IF EXISTS anon_user_cleanup ON anon_users;
DROP FUNCTION IF EXISTS user_cleanup();

ALTER TABLE exercise
    ADD COLUMN solution_language_id INTEGER REFERENCES language_support(language_id) NOT NULL DEFAULT 0;