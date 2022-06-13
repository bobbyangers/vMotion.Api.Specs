@CI @web @nonparallel
Feature: Health
    An anonymous user wants to get the Health page

Background:

@ignore
Scenario: Get the health page
  Given an anonymous using http web client
   When a GET request is sent to [/healthz]
   Then the response should be successful

Scenario: Get the swagger page
  Given an anonymous using http web client
  When a GET request is sent to [/index.html]
  Then the response should be successful

Scenario: Invoke the echo page
  Given a user with role [Any] using an http web client
    And a payload request
    | title      | message   |
    | some title | some data |
   When a POST request is sent to [/api/auth/echo]
   Then the response should be successful

@ignore
Scenario: Get error page
  Given an anonymous using http web client
   When a GET request is sent to [/api/reset/error]
   Then the response status should be InternalServerError