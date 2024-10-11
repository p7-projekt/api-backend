-- Step 1: Insert instructors first
INSERT INTO instructor (email, password, name)
VALUES
    ('john.doe@example.com', 'password123', 'John Doe'),
    ('jane.smith@example.com', 'securepass', 'Jane Smith');

-- Step 2: Insert sessions
INSERT INTO session (title, description, expirationtime_utc)
VALUES
    ('Haskell Basics', 'Introduction to basic Haskell programming', '2024-12-31 23:59:59'),
    ('Advanced Algorithms in Haskell', 'A session focusing on complex algorithms in Haskell', '2024-12-15 23:59:59'),
    ('Functional Data Structures', 'Session on functional data structures in Haskell', '2024-11-30 23:59:59');

-- Step 3: Insert exercises (with valid author_id)
INSERT INTO exercise (author_id, title, description, solution)
VALUES
    ((SELECT instructor_id FROM instructor WHERE email = 'john.doe@example.com'), 'Reverse a string', 'Write a function to reverse a string', 'reverseString :: String -> String\nreverseString s = reverse s'),
    ((SELECT instructor_id FROM instructor WHERE email = 'john.doe@example.com'), 'Is Prime', 'Write a function to check if a number is prime', 'isPrime :: Int -> Bool\nisPrime n = n > 1 && all (\\i -> n `mod` i /= 0) [2..floor (sqrt (fromIntegral n))]'),
    ((SELECT instructor_id FROM instructor WHERE email = 'jane.smith@example.com'), 'Factorial Number', 'Write a function to compute the factorial of a number', 'factorial :: Int -> Int\nfactorial 0 = 1\nfactorial n = n * factorial (n-1)');

-- Step 4: Map exercises to sessions in the exercise_in_session table
INSERT INTO exercise_in_session (exercise_id, session_id)
VALUES
    ((SELECT exercise_id FROM exercise WHERE title = 'Reverse a string'), (SELECT session_id FROM session WHERE title = 'Haskell Basics')),
    ((SELECT exercise_id FROM exercise WHERE title = 'Is Prime'), (SELECT session_id FROM session WHERE title = 'Advanced Algorithms in Haskell')),
    ((SELECT exercise_id FROM exercise WHERE title = 'Factorial Number'), (SELECT session_id FROM session WHERE title = 'Advanced Algorithms in Haskell')),
    ((SELECT exercise_id FROM exercise WHERE title = 'Factorial Number'), (SELECT session_id FROM session WHERE title = 'Functional Data Structures'));

-- Step 5: Insert students into the student table (with valid session_id)
INSERT INTO student (session_id)
VALUES
    ((SELECT session_id FROM session WHERE title = 'Haskell Basics')),
    ((SELECT session_id FROM session WHERE title = 'Advanced Algorithms in Haskell')),
    ((SELECT session_id FROM session WHERE title = 'Functional Data Structures')),
    ((SELECT session_id FROM session WHERE title = 'Haskell Basics'));

-- Step 6: Insert solved exercises data (with valid student_id and exercise_id)
INSERT INTO solved (student_id, exercise_id)
VALUES
    (1, (SELECT exercise_id FROM exercise WHERE title = 'Reverse a string')),
    (2, (SELECT exercise_id FROM exercise WHERE title = 'Is Prime')),
    (3, (SELECT exercise_id FROM exercise WHERE title = 'Factorial Number')),
    (4, (SELECT exercise_id FROM exercise WHERE title = 'Reverse a string'));

-- Step 7: Insert test cases for exercises
INSERT INTO testcase (exercise_id, testcase_no)
VALUES
    ((SELECT exercise_id FROM exercise WHERE title = 'Reverse a string'), 1),
    ((SELECT exercise_id FROM exercise WHERE title = 'Is Prime'), 1),
    ((SELECT exercise_id FROM exercise WHERE title = 'Factorial Number'), 1);

-- Step 8: Insert test case parameters
INSERT INTO testcase_parameter (testcase_id, arg_num, parameter_type, parameter_value, is_output)
VALUES
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Reverse a string')), 1, 'string', 'hello', FALSE),
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Reverse a string')), 1, 'string', 'olleh', TRUE),
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Is Prime')), 1, 'integer', '7', FALSE),
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Is Prime')), 1, 'boolean', 'true', TRUE),
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Factorial Number')), 1, 'integer', '5', FALSE),
    ((SELECT testcase_id FROM testcase WHERE exercise_id = (SELECT exercise_id FROM exercise WHERE title = 'Factorial Number')), 1, 'integer', '120', TRUE);
