@CI @web @mongoDb @nonparallel
Feature: Member
    In order to interact with the gMotion system
    As a regular member
    I want to make calls

Background:
  Given a user with role [Customer] using an http mobile client
    And location and device headers are set


Scenario: Customer gets his information
   When a GET request is sent to [/client-api/members/me]
   Then the response should be successful

Scenario Outline: Member adds a picture to his profile
  Given a picture with ext <fileType> needs to be uploaded
   When a POST request is sent to [/client-api/members/me/picture]
   Then the response should be successful
    And blob storage upload was invoked

 Examples:
 | fileType |
 | PNG      |
 | JPG      |

Scenario: Customer updates general information
  Given a payload request
    | name      |
    | Joe Smith |
   When a PUT request is sent to [/client-api/members/me]
   Then the response should be successful
    And member record was updated

Scenario: Customer updates his device info
    And a payload request
    | deviceId                         | DeviceType |
    | d5b406d6175347b2a6f03a4b712dfff8 | ios        |
   When a PUT request is sent to [/client-api/members/me/device]
   Then the response should be successful
    And member record has device info

Scenario: Customer wants to set his password
  Given a payload request
    | password |
    | Abc!3214 |
  When a PUT request is sent to [/client-api/members/me/password]
  Then the response should be successful

Scenario: Customer creates a new case
  Given a credit card exists
   Given a create case request
   When a POST request is sent to [/client-api/members/me/cases]
   Then the response should be successful
    And member record has device info
    And call record has device info

Scenario: Customer creates a new call
  Given a case exists with no active calls
  Given a create call request
   When a POST request is sent to [/client-api/members/me/calls]
   Then the response should be successful

Scenario: Customer wants to logout
  Given configure authentication service
  Given a payload request
    | token          | tokenHint     |
    | sometokenvalue | refresh_token |
  When a POST request is sent to [/client-api/members/me/logout]
  Then the response should be successful
