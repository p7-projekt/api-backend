INSERT INTO role(name) VALUES ('Student');

CREATE TABLE language_support(
    id SERIAL PRIMARY KEY,
    language VARCHAR(25) NOT NULL,
    version VARCHAR(25) NOT NULL
);

INSERT INTO language_support(language, version) VALUES ('Haskell', '9.8.2');
INSERT INTO language_support(language, version) VALUES ('Python', 'someVersion');

CREATE TABLE solution (
    id SERIAL PRIMARY KEY,
    solution TEXT NOT NULL,
    language INTEGER REFERENCES language_support(id) NOT NULL
);

ALTER TABLE solved RENAME TO submission;
ALTER TABLE submission 
    ADD COLUMN solution_id INTEGER REFERENCES solution(id) ON DELETE CASCADE 


