Feature: A Teacher creates a student
    Scenario: Teacher creates one student successfully
        Given a teacher is authenticated
        When the teacher sends creates a student with valid student data
        Then the response should be 201
        And the response should contain the student's email


