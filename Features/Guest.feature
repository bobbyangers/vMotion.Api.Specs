@CI @web @mongoDb @nonparallel
Feature: Guest
    In order to interact with the gMotion system
    As a regular member
    I want to make calls

Background:
  Given a user with role [Guest] using an http mobile client
    And location and device headers are set

Scenario: Guest gets his information
   When a GET request is sent to [/client-api/members/me]
   Then the response should be successful

Scenario Outline: Guest adds a picture to his profile
  Given a picture with ext JPG needs to be uploaded
   When a POST request is sent to [/client-api/members/me/picture]
   Then the response status should be Forbidden
    And blob storage upload was NOT invoked

Scenario: Guest updates general information
  Given a payload request
    | name      |
    | Joe Smith |
   When a PUT request is sent to [/client-api/members/me]
   Then the response status should be Forbidden


Scenario: Guest updates his device info
    And a payload request
    | deviceId                         | deviceType |
    | d5b406d6175347b2a6f03a4b712dfff8 | ios        |
   When a PUT request is sent to [/client-api/members/me/device]
   Then the response should be successful
    And guest record has device info

Scenario: Guest creates a new case
   Given a create case request
   When a POST request is sent to [/client-api/members/me/cases]
   Then the response should be successful
    And guest record has device info
    And call record has device info

Scenario: Guest creates a new call
  Given a case exists with no active calls
  Given a create call request
   When a POST request is sent to [/client-api/members/me/calls]
   Then the response should be successful


Scenario: Guest sends a notification message to staff
  Given guest has a current call set
  Given a payload request
         | data          |
         | { 'data': 3 } |
   When a POST request is sent to [/client-api/members/me/calls/in-progress/staff/events]
   Then the response should be successful
    And staff was notified

Scenario: Guest declines the current call
  Given guest has a current call set
    And call status is set to [Active]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/decline]
   Then the response should be successful
    And backoffice was notified
    And call status must be [Done]

Scenario: Guest joins the current call
  Given guest has a current call set
  And call status is set to [Waiting]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/join]
   Then the response should be successful
    And staff was notified
    And call status must be [WaitingForOther]

Scenario: Guest cancels the current call
  Given guest has a current call set
    And call status is set to [Waiting]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/cancel]
   Then the response should be successful
    And backoffice was notified
    And call status must be [Waiting]

Scenario: Guest hangs up the current call
  Given guest has a current call set
    And call status is set to [Active]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/hang-up]
   Then the response should be successful
    And backoffice was notified
    And staff was notified
    And call status must be [Done]

Scenario: Guest gets all his cases
  Given guest has a current call set
   When a GET request is sent to [/client-api/members/me/cases]
   Then the response should be successful
    And a list should have at least 1

Scenario: Guest gets the in-progress case info
  Given guest has a current call set
    And case is assigned
   When a GET request is sent to [/client-api/members/me/cases/in-progress]
   Then the response should be successful
    And the call info is not null

Scenario: Guest gets call in progress when no current call set
  Given guest has no current call set
   When a GET request is sent to [/client-api/members/me/cases/in-progress]
   Then the response status should be NotFound

Scenario: Guest gets a specific case info
  Given guest has a current call set
   When a GET request is sent to [/client-api/members/me/cases/{caseId}]
   Then the response should be successful

Scenario: Guest adds a note to a case
  Given guest has a current call set
  Given a payload request
    | note             |
    | some text to add |
  When a POST request is sent to [/client-api/members/me/cases/{caseId}/notes]
  Then the response status should be Forbidden

Scenario: Guest gets case notes for a case
  Given guest has a current call set
    And setup 2 notes for case
   When a GET request is sent to [/client-api/members/me/cases/{caseId}/notes]
   Then the response should be successful
    And case notes has 2

Scenario: Guest gets the calls history
  Given guest has a current call set
   When a GET request is sent to [/client-api/members/me/calls/{callId}/history]
   Then the response status should be Forbidden

Scenario: Guest reads pickup workflow information
  Given guest has a current call set
    And pickup workflow data exists
   When a GET request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response status should be Forbidden
   #Then the response should be successful

Scenario: Guest saves new pickup workflow information
  Given guest has a current call set
    And pickup workflow is empty
    And pickup workflow payload is set
   When a PUT request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response status should be Forbidden
   #Then the response should be successful

Scenario: Guest saves override pickup workflow information
  Given guest has a current call set
    And pickup workflow data exists
    And pickup workflow payload is set
   When a PUT request is sent to [/client-api/members/me/cases/{caseId}/pickup-workflow]
   Then the response status should be Forbidden
   #Then the response should be successful

# by default => all archived = false
Scenario: Guest gets all his notifications
   When a GET request is sent to [/client-api/members/me/notifications]
   Then the response status should be Forbidden


Scenario: Guest adds a notification
  Given a payload request
     | notificationType | operatorId                           | caseId                               | description | data  |
     | alert            | 00E0A000-0000-0000-0001-000000000011 | CA110000-0000-0000-0001-000000000001 | some data   | extra |
   When a POST request is sent to [/client-api/members/me/notifications]
   Then the response status should be Forbidden

Scenario: Guest marks a notification as read
  Given a notification record exists
   When a PUT request is sent to [/client-api/members/me/notifications/{notificationId}/read]
   Then the response status should be Forbidden

Scenario: Guest updates a notification
  Given a notification record exists
   When a PUT request is sent to [/client-api/members/me/notifications/{notificationId}]
   Then the response status should be Forbidden

Scenario: Guest deletes a notification
  Given a notification record exists
   When a DELETE request is sent to [/client-api/members/me/notifications/{notificationId}]
   Then the response status should be Forbidden

Scenario: Guest gets an operator by code
  Given an operator with code [B3D5] exists
   When a GET request is sent to [/client-api/members/me/operators/b3d5]
   Then the response status should be Ok

Scenario: Guest retrieves all his operators
   When a GET request is sent to [/client-api/members/me/operators?]
   Then the response status should be Ok
    And a list should have 0

Scenario: Guest wants to link to an operator using digit 4 alpha numeric codes
  Given an operator with code [B2V4] exists
   When a PUT request is sent to [/client-api/members/me/operators/B2V4]
   Then the response status should be Forbidden

Scenario: Guest wants to unlink from an operator
   When a DELETE request is sent to [/client-api/members/me/operators/b3d5]
   Then the response status should be Forbidden

Scenario: Guest wants to set his password
  Given a payload request
    | password |
    | Abc!3214 |
  When a PUT request is sent to [/client-api/members/me/password]
  Then the response status should be Forbidden

Scenario: Guest wants to logout
  Given a payload request
    | token          | tokenHint     |
    | sometokenvalue | refresh_token |
  When a POST request is sent to [/client-api/members/me/logout]
  Then the response should be successful
