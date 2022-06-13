@CI @web @mongoDb @nonparallel
Feature: MemberCalls
    As a regular member
    I want to manage calls

Background:
  Given a user with role [Customer] using a mobile client
    And a staff exists
    And the member has a current call set

Scenario: Customer sends a notification message to staff
   Given a payload request
         | data          |
         | { 'data': 3 } |
   When a POST request is sent to [/client-api/members/me/calls/in-progress/staff/events]
   Then the response should be successful
   And staff was notified

Scenario: Customer declines the current call
  Given call status is set to [Active]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/decline]
   Then the response should be successful
    And backoffice was notified
    And call status must be [Done]

Scenario: Customer joins the current call
  Given call status is set to [Waiting]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/join]
   Then the response should be successful
    And staff was notified
    And call status must be [WaitingForOther]

Scenario: Customer cancels the current call
  Given call status is set to [Waiting]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/cancel]
   Then the response should be successful
    And backoffice was notified
    And call status must be [Waiting]

Scenario: Customer hangs up the current call
  Given call status is set to [Active]
   When a PUT request is sent to [/client-api/members/me/calls/{callId}/hang-up]
   Then the response should be successful
    And backoffice was notified
    And staff was notified
    And call status must be [Done]

Scenario: Customer gets the calls history
   When a GET request is sent to [/client-api/members/me/calls/{callId}/history]
   Then the response should be successful