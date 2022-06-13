@CI @web @mongoDb @nonparallel
Feature: Staff Manager
    In order to manage other staff
    As a manager
    I want to be update and manage other staff data

Background:
  Given a user with role [Manager] using an http web client

Scenario: Manager gets all operators
   When a GET request is sent to [/api/operators]
   Then the response should be successful

Scenario: Manager gets an operator
   When a GET request is sent to [/api/operators/{operatorId}]
   Then the response should be successful

Scenario: Manager updates the operator's picture
  Given picture needs to be uploaded
   When a POST request is sent to [/api/operators/{operatorId}/picture]
   Then the response should be successful
    And operator picture was updated
    And blob storage upload was invoked

Scenario: Manager creates an operator
  Given a payload request
    | name         | email                  | phoneNumber  | address  |
    | operatorName | somebody@somewhere.com | 555-525-1234 | somwhere |
    When a POST request is sent to [/api/operators]
    Then the response status should be Created
     And headers should have location

Scenario: Manager update an operator
  Given an operator with code [ZZZ1] exists
    And a payload request
    | name         | code |
    | operatorName | X001 |
    When a PUT request is sent to [/api/operators/{operatorId}]
    Then the response status should be ResetContent

@ignore
Scenario: Manager updates an operator code
  Given an operator with code [ZZZ2] exists
    And a payload request
    | code |
    | X991 |
    When a PUT request is sent to [/api/operators/{operatorId}/code]
    Then the response status should be ResetContent

@ignore
Scenario: Manager sets an operator code
  Given a payload request
    | code |
    | X1X2 |
   When a PUT request is sent to [/api/operators/{operatorId}/code]
   Then the response should be successful

Scenario: Manager adds a role to a staff
   Given role [00000000-0000-0000-0001-000000000011] exists
   Given another staff exists
   Given an add to role request
    When a POST request is sent to [/api/roles/{roleId}/staff]
    Then the response should be successful
     And staff record roles were added to [00000000-0000-0000-0001-000000000011]

Scenario: Manager removes role from a staff
   Given another staff exists
   Given role [00000000-0000-0000-0001-000000000011] exists
    When a DELETE request is sent to [/api/roles/{roleId}/staff/{anotherStaffId}]
    Then the response should be successful
     And other staff role [00000000-0000-0000-0001-000000000011] record removed

Scenario: Manager gets another staff
   Given another staff exists
    When a GET request is sent to [/api/staffs/{anotherStaffId}]
    Then the response status should be OK
     And response body is not empty

Scenario: Manager creates another staff
   Given a create staff request
   Given configure authentication service
   When a POST request is sent to [/api/staffs]
   Then the response should be successful
    And staff record was created

@ignore
Scenario: Manager updates another staff
  Given another staff exists
  Given a payload request
    | name      |
    | Joe Smith |
   When a PUT request is sent to [/api/staffs/{anotherStaffId}]
   Then the response should be successful
    And other staff record was updated

Scenario: Manager updates another staff usertType
  Given another staff exists
   When a PUT request is sent to [/api/staffs/{anotherStaffId}/userType]
   Then the response should be successful

Scenario: Manager deletes another staff
  Given another staff exists
   When a DELETE request is sent to [/api/staffs/{anotherStaffId}]
   Then the response should be successful

Scenario: Manager wants to delete a member
  Given a member exists
   When a DELETE request is sent to [/api/members/{memberId}]
   Then the response should be successful
    And the member was deleted

Scenario: Manager gets staff online
    When a GET request is sent to [/api/staffs/online]
    Then the response should be successful

Scenario: Manager gets all roles
    When a GET request is sent to [/api/roles?count=3]
    Then the response should be successful
     And a list of operators
     #And a custom list should have 3

Scenario: Manager creates a role
    And a CreateRoleCommand payload
    When a POST request is sent to [/api/roles]
    Then the response should be successful

Scenario: Manager gets one role
   Given role [00000000-0000-0000-9999-000000000011] exists
    When a GET request is sent to [/api/roles/{roleId}/]
    Then the response should be successful

Scenario: Manager updates a role
   Given role [00000000-0000-0000-9999-000000000011] exists
    And a UpdateRoleCommand payload
    When a PUT request is sent to [/api/roles/{roleId}/]
    Then the response should be successful

Scenario: Manager deletes a role
   Given role [00000000-0000-0000-9999-000000000011] exists
    When a DELETE request is sent to [/api/roles/{roleId}/]
    Then the response should be successful
