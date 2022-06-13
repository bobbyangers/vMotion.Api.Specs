@CI @web @mongoDb @nonparallel
Feature: MemberCases
    As a regular member
    I want to manage cases

Background:
  Given a user with role [Customer] using an http mobile client
    And the member has a current call set

Scenario: Customer gets all his cases
   When a GET request is sent to [/client-api/members/me/cases]
   Then the response should be successful
    And a list should have at least 1

Scenario: Customer gets the in-progress case info
   Given case is assigned
   When a GET request is sent to [/client-api/members/me/cases/in-progress]
   Then the response should be successful
    And the call info is not null

Scenario: Customer gets a specific case info
   When a GET request is sent to [/client-api/members/me/cases/{caseId}]
   Then the response should be successful

Scenario: Customer adds a note to a case
  Given a payload request
    | note             |
    | some text to add |
  When a POST request is sent to [/client-api/members/me/cases/{caseId}/notes]
  Then the response should be successful

Scenario: Customer gets case notes for a case
  Given setup 2 notes for case
   When a GET request is sent to [/client-api/members/me/cases/{caseId}/notes]
   Then the response should be successful
    And case notes has 2

Scenario: Customer reads pickup workflow information
  Given pickup workflow data exists
   When a GET request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: Customer saves new pickup workflow information
  Given pickup workflow is empty
    And pickup workflow payload is set
   When a PUT request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: Customer saves override pickup workflow information
  Given pickup workflow data exists
    And pickup workflow payload is set
   When a PUT request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: Customer sets the selected card
  Given a credit card exists
  Given a payload request
    | id        |
    | {cCardId} |
  When a PUT request is sent to [/client-api/members/me/cases/{caseId}/selectedCard]
  Then the response should be successful