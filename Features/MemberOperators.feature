@CI @web @mongoDb @nonparallel
Feature: MemberOperators
     in order to manage my links to given providers

Background:
  Given a user with role [Customer] using an http mobile client

Scenario: A member wants to get an operator by code
  Given an operator with code [B3D5] exists
   When a GET request is sent to [/client-api/members/me/operators/b3d5]
   Then the response status should be Ok

Scenario Outline: Customer links to an operator using digit 4 alpha numeric codes
  Given an operator with code [<code>] exists
   When a PUT request is sent to [/client-api/members/me/operators/<code>]
   Then the response status should be Accepted
    And member is linked to operator with code [<code>]

    Examples:
    | code |
    | B2V4 |
    | C2V4 |

Scenario: Customer retrieves all his operators
  Given an operator with code [B3D5] exists
  Given an operator with code [C4E6] exists
    And clear linked operators in member
    And member is linked to [B3D5]
    And member is linked to [C4E6]
   When a GET request is sent to [/client-api/members/me/operators?]
   Then the response status should be Ok
    And a list should have 2

Scenario: Customer links to an operator with a invalid code
   When a PUT request is sent to [/client-api/members/me/operators/XXXX]
   Then the response status should be NotFound

Scenario: Customer unlinks from an operator
  Given an operator with code [B3D5] exists
  Given member is linked to [B3D5]
   When a DELETE request is sent to [/client-api/members/me/operators/b3d5]
   Then the response status should be Accepted
    And member is not linked to operator with code [B3D5]

Scenario: Customer wants to unlink from an operator he is not linked to
  Given an operator with code [B3D5] exists
   When a DELETE request is sent to [/client-api/members/me/operators/b3d5]
   Then the response status should be PreconditionFailed
    And member is not linked to operator with code [B3D5]


