INSERT INTO role(name) VALUES ('Student');

CREATE TABLE language_support(
    language_id SERIAL PRIMARY KEY,
    language VARCHAR(25) NOT NULL,
    version VARCHAR(25) NOT NULL
);

INSERT INTO language_support(language, version) VALUES ('haskell', '9.8.2');
INSERT INTO language_support(language, version) VALUES ('python', 'someVersion');

CREATE TABLE solution (
    solution_id SERIAL PRIMARY KEY,
    solution TEXT NOT NULL,
    language_id INTEGER REFERENCES language_support(language_id) NOT NULL
);

ALTER TABLE solved RENAME TO submission;
ALTER TABLE submission 
    ADD COLUMN solution_id INTEGER REFERENCES solution(solution_id) ON DELETE CASCADE; 

CREATE TABLE language_in_session (
    session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE,
    language_id INTEGER REFERENCES language_support(language_id) ON DELETE CASCADE,
    PRIMARY KEY (session_id, language_id)
);





