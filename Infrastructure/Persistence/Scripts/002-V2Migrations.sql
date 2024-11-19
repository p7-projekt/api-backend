INSERT INTO role(name) VALUES ('Student')

CREATE TABLE classroom (
	id SERIAL PRIMARY KEY,
	title VARCHAR(100),
	owner INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL
);

CREATE TABLE session_in_classroom(
	classroom_id INTEGER REFERENCES classroom(id) ON DELETE CASCADE NOT NULL,
	session_id INTEGER REFERENCES session(id) ON DELETE CASCADE NOT NULL,
	active BOOLEAN NOT NULL,
	PRIMARY KEY(classroom_id, session_id)
);

CREATE TABLE student_in_classroom(
	student_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
	classroom_id INTEGER REFERENCES classroom(id) ON DELETE CASCADE NOT NULL,
	PRIMARY KEY(student_id, classroom_id)
);