@CI @web @mongoDb @nonparallel
Feature: Call Agent Members
    As a call agent
    I want to execute a series of actions on members

Background:
  Given a user with role [Agent] using an http web client
    And a member exists

Scenario: An Agent get a list of members
   When a GET request is sent to [/api/members?count=5]
   Then the response should be successful

Scenario: An Agent updates the member
  Given a member update request
   When a PUT request is sent to [/api/members/{memberId}]
   Then the response should be successful

Scenario: An Agent adds a notification to a member
  Given a payload request
    | notificationType | operatorId   | description |
    | alert            | {operatorId} | some data   |
   When a POST request is sent to [/api/members/{memberId}/notifications]
   Then the response status should be Accepted
    And member was notified via signalr

Scenario: An Agent sends signalr message to member
  Given a signalr payload
  When a POST request is sent to [/api/members/{memberId}/events]
  Then the response should be successful
   And member was notified via signalr

Scenario: An Agent sends an APNS to a member
  Given a payload request
    | title      | message   |
    | some title | some data |
   When a PUT request is sent to [/api/members/{memberId}/device]
   Then the response should be successful
   And member was notified via apn

Scenario: An Agent sends an APNS notification but device id is invalid
  Given a payload request
    | title      | message   |
    | some title | some data |
    And notification service returned bad device
   When a PUT request is sent to [/api/members/{memberId}/device]
   Then the response status should be NotFound

Scenario: An Agent fetches the last payments
  Given payments data exists
  When a GET request is sent to [/api/members/{memberId}/last-payments]
  Then the response should be successful
    And list is not empty

@ignore
Scenario Outline: An Agent sets a members picture
  Given a picture with ext <fileType> needs to be uploaded
   When a POST request is sent to [/api/members/{memberId}/set-picture]
   Then the response should be successful
    And blob storage upload was invoked

 Examples:
 | fileType |
 | PNG      |
 | JPG      |


Scenario: An Agent fetches one credit card
  Given a credit card exists
  When a GET request is sent to [/api/members/{memberId}/creditcards/{cCardId}]
  Then the response should be successful

Scenario: An Agent fetches the credit cards
  Given a credit card exists
  When a GET request is sent to [/api/members/{memberId}/creditcards]
  Then the response should be successful
    And list is not empty

Scenario: An Agent updates the validation field for a credit card
  Given a credit card exists
    And an update credit card validation payload
  When a PUT request is sent to [/api/members/{memberId}/creditcards/{cCardId}/validation]
  Then the response should be successful
