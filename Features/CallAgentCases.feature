@CI @web @mongoDb @nonparallel
Feature: CallAgentCases
    As a call agent
    I want to execute a series of actions on cases

Background:
  Given a user with role [Agent] using an http web client
    And a member exists
    And the member has a current call set

Scenario: An Agent gets all cases for a member
   When a GET request is sent to [/api/cases/members/{memberId}?count=10]
   Then the response should be successful
    And a list should have at least 1

Scenario: An Agent gets a case
   When a GET request is sent to [/api/cases/{caseId}]
   Then the response should be successful

Scenario: An Agent sets a member's case to validated
  Given current call is set for staff
   When a PUT request is sent to [/api/cases/{caseId}/info-validated]
   Then the response status should be Accepted
    And case record was updated

Scenario: An Agent adds a note to a case
  Given current call is set for staff
  Given a payload request
    | note             |
    | some text to add |
   When a POST request is sent to [/api/cases/{caseId}/notes]
   Then the response should be successful
    And case note record was created

Scenario: An Agent updates a note
  Given current call is set for staff
    And a 1 notes for case
  Given a payload request
    | note             | visibleTo |
    | some text to add | customer  |
   When a PUT request is sent to [/api/cases/{caseId}/notes/{noteId}]
   Then the response should be successful

Scenario: An Agent reads a note
  Given a 1 notes for case
   When a GET request is sent to [/api/cases/{caseId}/notes/{noteId}]
   Then the response should be successful

Scenario: An Agent deletes notes from a case
  Given current call is set for staff
    And a 1 notes for case
   When a DELETE request is sent to [/api/cases/{caseId}/notes/{noteId}]
   Then the response should be successful
    And note record was deleted

Scenario: An Agent reads all notes from a case
  Given a 3 notes for case
   When a GET request is sent to [/api/cases/{caseId}/notes]
   Then the response should be successful
    And a list should have 3

Scenario: An Agent reads pickup workflow information
  Given pickup workflow data exists
   When a GET request is sent to [/api/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: An Agent saves new pickup workflow information
  Given current call is set for staff
    And pickup workflow is empty
    And pickup workflow payload is set
   When a PUT request is sent to [/api/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: An Agent saves override pickup workflow information
  Given current call is set for staff
    And pickup workflow data exists
    And pickup workflow payload is set
   When a PUT request is sent to [/api/cases/{caseId}/pickup-workflow]
   Then the response should be successful

Scenario: An Agent closes a case
   When a PUT request is sent to [/api/cases/{caseId}/close]
   Then the response should be successful

Scenario: An Agent adds a picture
  Given current call is set for staff
    And some pictures need to be uploaded
   When a POST request is sent to [/api/cases/{caseId}/picture]
   Then the response should be successful
    And blob storage upload was invoked

Scenario: An Agent adds a picture at position 99
  Given current call is set for staff
    And a picture needs to be uploaded
   When a PUT request is sent to [/api/cases/{caseId}/picture/99]
   Then the response should be successful
    And blob storage upload was invoked

@ignore
Scenario: An Agent gets the case's call history
  Given current call is set for staff
   When a GET request is sent to [/api/cases/{caseId}/call-history]
   Then the response should be successful
    And list is not empty

@ignore
Scenario: An Agent queries call history by timeRange
  Given calls exists recently ended
   When a GET request is sent to [/api/cases/call-history?timeRange={lastWeek}]
   Then the response should be successful
    And list is not empty

@ignore
Scenario: An Agent queries call history by memberId
  Given calls exists recently ended
   When a GET request is sent to [/api/cases/call-history?memberId={memberId}]
   Then the response should be successful
    And list is not empty

@ignore
Scenario: An Agent queries call history by memberId and time range
  Given calls exists recently ended
   When a GET request is sent to [/api/cases/call-history?memberId={memberId}&timeRange={lastWeek}]
   Then the response should be successful
    And list is not empty

@ignore
Scenario: An Agent queries call history by assignedTo
  Given calls exists assignedTo
   When a GET request is sent to [/api/cases/call-history?assignedTo={anotherStaffId}]
   Then the response should be successful
    And list is not empty

@ignore
Scenario: An Agent queries call history by assignedTo and time range
   Given calls exists assignedTo
   When a GET request is sent to [/api/cases/call-history?assignedTo={anotherStaffId}&timeRange={lastWeek}]
   Then the response should be successful
    And list is not empty
