CREATE TABLE
    exercise (
        exercise_id SERIAL PRIMARY KEY,
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
        session_id INTEGER REFERENCES session (session_id) NOT NULL
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
        exercise_id INTEGER REFERENCES exercise (exercise_id) ON DELETE CASCADE NOT NULL
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

-- Insert dummy data into the exercise table
INSERT INTO
    exercise (description, solution)
VALUES
    (
        'Write a function to reverse a string',
        'reverseString :: String -> String\nreverseString s = reverse s'
    ),
    (
        'Write a function to check if a number is prime',
        'isPrime :: Int -> Bool\nisPrime n = n > 1 && all (\\i -> n `mod` i /= 0) [2..floor (sqrt (fromIntegral n))]'
    ),
    (
        'Write a function to compute the factorial of a number',
        'factorial :: Int -> Int\nfactorial 0 = 1\nfactorial n = n * factorial (n-1)'
    );

-- Insert dummy data into the session table
INSERT INTO
    session (title, description, expirationtime_utc)
VALUES
    (
        'Haskell Basics',
        'Introduction to basic Haskell programming',
        '2024-12-31 23:59:59'
    ),
    (
        'Advanced Algorithms in Haskell',
        'A session focusing on complex algorithms in Haskell',
        '2024-12-15 23:59:59'
    ),
    (
        'Functional Data Structures',
        'Session on functional data structures in Haskell',
        '2024-11-30 23:59:59'
    );

-- Insert data into exercise_in_session table (Mapping exercises to sessions)
INSERT INTO
    exercise_in_session (exercise_id, session_id)
VALUES
    (1, 1), -- Reverse string in Haskell Basics
    (2, 2), -- Prime check in Advanced Algorithms in Haskell
    (3, 2), -- Factorial in Advanced Algorithms in Haskell
    (3, 3);

-- Factorial in Functional Data Structures
-- Insert dummy data into the student table
INSERT INTO
    student (session_id)
VALUES
    (1), -- Student 1 in Haskell Basics
    (2), -- Student 2 in Advanced Algorithms in Haskell
    (3), -- Student 3 in Functional Data Structures
    (1);

-- Student 4 in Haskell Basics
-- Insert data into solved table (Which student solved which exercise)
INSERT INTO
    solved (student_id, exercise_id)
VALUES
    (1, 1), -- Student 1 solved Reverse string
    (2, 2), -- Student 2 solved Prime check
    (3, 3), -- Student 3 solved Factorial in Functional Data Structures
    (4, 1);

-- Student 4 solved Reverse string
-- Insert dummy data into the testcase table
INSERT INTO
    testcase (exercise_id)
VALUES
    (1), -- Test case for reverse string
    (2), -- Test case for prime check
    (3);

-- Test case for factorial
-- Insert dummy data into testcase_parameter table
INSERT INTO
    testcase_parameter (
        testcase_id,
        arg_num,
        parameter_type,
        parameter_value,
        is_output
    )
VALUES
    (1, 1, 'string', 'hello', FALSE), -- Input for reverse string
    (1, 1, 'string', 'olleh', TRUE), -- Expected output for reverse string
    (2, 1, 'integer', '7', FALSE), -- Input for prime check
    (2, 1, 'boolean', 'true', TRUE), -- Expected output for prime check
    (3, 1, 'integer', '5', FALSE), -- Input for factorial
    (3, 1, 'integer', '120', TRUE);

-- Expected output for factorial