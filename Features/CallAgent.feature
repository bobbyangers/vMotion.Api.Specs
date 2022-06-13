@CI @web @mongoDb @nonparallel
Feature: Call Agent
    As an agent
    I want to execute a series of actions

Background:
  Given a user with role [Agent] using an http web client
    And a member exists

Scenario: An Agent sends a heartbeat
   When a POST request is sent to [/api/staffs/heartbeat]
   Then the response should be successful
    And staffOnline record is updated

Scenario: An Agent gets his webrole
   When a GET request is sent to [/api/staffs/me/webrole]
   Then the response should be successful

Scenario: An Agent retrieves the list of staff
  Given a staff exists
   When a GET request is sent to [/api/staffs]
   Then the response should be successful
    And list is not empty

Scenario: An Agent clears his currentcall field
   When a DELETE request is sent to [/api/staffs/{staffId}/currentCall]
   Then the response should be successful