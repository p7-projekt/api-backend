CREATE TABLE
    instructor (
        instructor_id SERIAL PRIMARY KEY,
        email VARCHAR(50) NOT NULL,
        password VARCHAR(100) NOT NULL,
        name VARCHAR(100) NOT NULL
    );

CREATE TABLE
    exercise (
        exercise_id SERIAL PRIMARY KEY,
        author_id INTEGER REFERENCES instructor (instructor_id) ON DELETE CASCADE NOT NULL,
        title TEXT NOT NULL,
        description TEXT,
        solution TEXT NOT NULL
    );

CREATE TABLE
    session (
        session_id SERIAL PRIMARY KEY,
        title VARCHAR(50) NOT NULL,
        description TEXT,
        expirationtime_utc TIMESTAMP NOT NULL
    );

CREATE TABLE
    exercise_in_session (
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE,
        session_id INTEGER REFERENCES session (session_id) ON DELETE CASCADE,
        PRIMARY KEY (exercise_id, session_id)
    );

CREATE TABLE
    student (
        student_id SERIAL PRIMARY KEY,
        session_id INTEGER REFERENCES session (session_id) ON DELETE CASCADE NOT NULL
    );

CREATE TABLE
    solved (
        student_id INTEGER REFERENCES student (student_id) ON DELETE CASCADE,
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE,
        PRIMARY KEY (student_id, exercise_id)
    );

CREATE TABLE
    testcase (
        testcase_id SERIAL PRIMARY KEY,
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE NOT NULL,
        testcase_no INTEGER NOT NULL
    );

CREATE TABLE
    testcase_parameter (
        parameter_id SERIAL PRIMARY KEY,
        testcase_id INTEGER REFERENCES testcase (testcase_id) ON DELETE CASCADE NOT NULL,
        arg_num INTEGER NOT NULL,
        parameter_type VARCHAR(20) NOT NULL,
        parameter_value VARCHAR(255) NOT NULL,
        is_output BOOLEAN NOT NULL
    );

-- Handle auth
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL
);
-- Consider created at / last login 

CREATE TABLE role (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);
-- Important to seed roles to ensure they exist

CREATE TABLE user_role (
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
    role_id INTEGER REFERENCES role(id) ON DELETE CASCADE NOT NULL,
    PRIMARY KEY (user_id, role_id)
);
