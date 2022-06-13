@CI @web @mongoDb @nonparallel
Feature: CallAgentCalls
    As an agent
    I want to execute a series of actions

Background:
  Given a user with role [Agent] using an http web client
    And a member exists
    And a call exists

Scenario: An Agent wants retrieve specific member
  When a GET request is sent to [/api/members/{memberId}]
  Then the response should be successful

Scenario: An Agent retrieves the list of staff
  Given a staff exists
   When a GET request is sent to [/api/staffs]
   Then the response should be successful
    And list is not empty

Scenario: An Agent takes a specific call
  Given no current call is set for staff
   When a PUT request is sent to [/api/calls/{callId}/take]
   Then the response should be successful
    And staff was notified
    And member was notified via signalr with message [RefreshCall]
    And backoffice was notified
    And staff current call was updated

Scenario: An Agent takes a next available call
  Given purge all calls
    And a call exists
  Given no current call is set for staff
   When a PUT request is sent to [/api/calls/take-next]
   Then the response should be successful
    And staff was notified
    And member was notified via signalr with message [RefreshCall]
    And backoffice was notified
    And staff current call was updated

Scenario: An Agent gets a specific call
  When a GET request is sent to [/api/calls/{callId}]
  Then the response should be successful

Scenario: An Agent gets call in progress
  Given current call is set for staff
   When a GET request is sent to [/api/calls/in-progress]
   Then the response should be successful

Scenario: An Agent add data to the call
  Given an add data to call request
    And current call is set for staff
   When a PATCH request is sent to [/api/calls/in-progress]
   Then the response should be successful
    And call has data set

Scenario: An Agent gets call in progress when current not set
  Given no current call is set for staff
  When a GET request is sent to [/api/calls/in-progress]
  Then the response status should be NotFound

Scenario: An Agent joins the call
 Given no current call is set for staff
  When a PUT request is sent to [/api/calls/{callId}/join]
  Then the response should be successful
   And member was notified via signalr with message [ReadyForCall]
   And call status must be [WaitingForOther]

Scenario: An Agent hangs up the call
  Given current call is set for staff
   When a PUT request is sent to [/api/calls/{callId}/hang-up]
   Then the response should be successful
    And member was notified via signalr with message [VideoDisconnected]
    And call status must be [Waiting]

Scenario: An Agent completes the call
  Given current call is set for staff
    And a complete request
   When a PUT request is sent to [/api/calls/{callId}/complete]
   Then the response should be successful
    And backoffice was notified
    And member was notified via signalr with message [RefreshCall]
    And staff current call was cleared
    And member current call was cleared

Scenario: An Agent ends the call
  Given current call is set for staff
   When a PUT request is sent to [/api/calls/{callId}/end]
   Then the response should be successful
    #And backoffice was notified
    #And member was notified via signalr with message [RefreshCall]

Scenario: An Agent gets the call history
  Given current call is set for staff
   When a GET request is sent to [/api/calls/{callId}/history]
   Then the response should be successful
    And list is not empty

Scenario: An Agent sends an Apns notification
  Given current call is set for staff
    And device info is saved for member
    And a payload request
    | title      | message   |
    | some title | some data |
   When a PUT request is sent to [/api/calls/{callId}/push-notification]
   Then the response should be successful
    And push notification service was invoked

Scenario: An Agent sends an Apns notification but device id is invalid
  Given current call is set for staff
    And device info is saved for member
  Given a payload request
    | title      | message   |
    | some title | some data |
    And current call is set for staff
    And notification service returned bad device
   When a PUT request is sent to [/api/calls/{callId}/push-notification]
   Then the response status should be PreConditionFailed

@ignore
Scenario: An Agent assigns a call to another staff
  Given an reassignment request to user
   When a PUT request is sent to [/api/calls/{callId}/assign-to]
   Then the response should be successful
    #And staff record was updated
    #And call was updated with assigned to
    And backoffice was notified
    And member was notified

@ignore
Scenario Outline: An Agent assigns a call to another staff group
  Given an reassignment request to type <user>
   When a PUT request is sent to [/api/calls/{callId}/assign-to]
   Then the response should be successful
    #And staff record was updated
    #And call was updated with assigned to
    And backoffice was notified
    And member was notified

    Examples:
    | user      |
    | Doctor    |
    | Nurse     |
    | CallAgent |

Scenario: An Agent creates a payment intent
 Given a member has a credit card set
 Given a payload request
    | amount | cardId    | currency | daysToHold |
    | 102.00 | {cCardId} | cad      | 7          |
  When a POST request is sent to [/api/calls/{callId}/payment-intent]
  Then the response should be successful

Scenario: An Agent creates a charge
 Given a member has a credit card set
 Given a payload request
    | amount | cardId    | currency |
    | 100.00 | {cCardId} | cad      |
  When a POST request is sent to [/api/calls/{callId}/charge-card]
  Then the response should be successful

Scenario: An Agent clears his currentcall field
   When a DELETE request is sent to [/api/staffs/{staffId}/currentCall]
   Then the response should be successful
