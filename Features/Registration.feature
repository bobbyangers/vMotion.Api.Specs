@CI @web @mongoDb @nonparallel
Feature: Registration
    As a potential new member
    Wants to register using the simple form

Background:
  Given an anonymous using http web client

Scenario: A user sends registration form
  Given complete registration data
    And configure authentication service
   When a POST request is sent to [/client-api/registration/me]
   Then the response should be successful
    And user was created
    And should receive a token

Scenario: A user registers with an incomplete registration
    Given incomplete registration data
     When a POST request is sent to [/client-api/registration/me]
     Then the response status should be BadRequest

Scenario: A user registers using an existing email
  Given registration data with existing email
   When a POST request is sent to [/client-api/registration/me]
   Then the response status should be Conflict

Scenario: A visitor registers as a guest
  Given a payload request
    | firstName | phoneNumber |
    | Joe       | 514555-1234 |
    And configure authentication service
   When a POST request is sent to [/client-api/registration/as-guest]
   Then the response should be successful
    And user was created
    And should receive a token