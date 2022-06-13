@CI @web @mongoDb @nonparallel
Feature: CallAgentOperator
    As an agent
    I want to execute a series of actions


Background:
  Given a user with role [Agent] using an http web client

Scenario: An Agent gets all operators
   When a GET request is sent to [/api/operators]
   Then the response should be successful

Scenario: An Agent get an operator
   When a GET request is sent to [/api/operators/{operatorId}]
   Then the response should be successful

Scenario: An Agent retrieves operators complete reason
   When a GET request is sent to [/api/operators/{operatorId}/complete-call-reasons]
   Then the response should be successful
    And list is not empty

Scenario: An Agent retrieves operators call intentions
   When a GET request is sent to [/api/operators/{operatorId}/call-intentions]
   Then the response should be successful
    And list is not empty

Scenario: An Agent gets his permissions
  When a GET request is sent to [/api/operators/my-permissions?]
  Then the response should be successful