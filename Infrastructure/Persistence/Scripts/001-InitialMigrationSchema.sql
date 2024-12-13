CREATE TABLE 
    users (
        id SERIAL PRIMARY KEY,
        created_at TIMESTAMP NOT NULL
);

CREATE TABLE
    session (
        session_id SERIAL PRIMARY KEY,
        title VARCHAR(50) NOT NULL,
        description TEXT,
        author_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
        expirationtime_utc TIMESTAMP NOT NULL,
        session_code VARCHAR(6) UNIQUE NOT NULL
);

CREATE TABLE 
    app_users (
        user_id INTEGER PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
        email VARCHAR(50) UNIQUE NOT NULL,
        name VARCHAR(100) NOT NULL,
        password_hash VARCHAR(255) NOT NULL
);

CREATE TABLE 
    anon_users (
        user_id INTEGER PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
        session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE NOT NULL
); 

CREATE TABLE
    exercise (
        exercise_id SERIAL PRIMARY KEY,
        author_id INTEGER REFERENCES users (id) ON DELETE CASCADE NOT NULL,
        title TEXT NOT NULL,
        description TEXT,
        solution TEXT NOT NULL
    );



CREATE TABLE
    exercise_in_session (
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE,
        session_id INTEGER REFERENCES session (session_id) ON DELETE CASCADE,
        PRIMARY KEY (exercise_id, session_id)
    );

CREATE TABLE
    solved (
        user_id INTEGER REFERENCES users (id) ON DELETE CASCADE,
        session_id INTEGER REFERENCES session(session_id) ON DELETE CASCADE NOT NULL,
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE NOT NULL,
        PRIMARY KEY (user_id, exercise_id)
    );

CREATE TABLE
    testcase (
        testcase_id SERIAL PRIMARY KEY,
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE NOT NULL,
        testcase_no INTEGER NOT NULL,
        public_visible BOOL NOT NULL
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

CREATE TABLE role (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);
-- Important to seed roles to ensure they exist
INSERT INTO role (name) VALUES ('Instructor'); 
INSERT INTO role (name) VALUES ('AnonymousUser');
CREATE TABLE user_role (
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL,
    role_id INTEGER REFERENCES role(id) ON DELETE CASCADE NOT NULL,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE refresh_token(
    id SERIAL PRIMARY KEY,
    token TEXT UNIQUE NOT NULL,
    expires TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE NOT NULL
);

CREATE FUNCTION user_cleanup() -- After trigger https://www.postgresql.org/docs/current/plpgsql-trigger.html OLD is the deleted record from anon_users
RETURNS TRIGGER AS $$
    BEGIN
        DELETE FROM users WHERE id = OLD.user_id;
        RETURN NULL;
    END;
    $$ LANGUAGE plpgsql;

CREATE TRIGGER anon_user_cleanup
    AFTER DELETE ON anon_users
    FOR EACH ROW
    EXECUTE FUNCTION user_cleanup();
    