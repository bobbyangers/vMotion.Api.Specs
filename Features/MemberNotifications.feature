@CI @web @mongoDb @nonparallel
Feature: MemberNotifications
    As a registered member
    I would like to manage my notifications

Background:
  Given a user with role [Customer] using a mobile client

# by default => all archived = false
Scenario: Customer wants to get all his notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead | IsArchived |
    | 00          | alert            | false  | false      |
    | 01          | alert            | false  | true       |
    | 02          | alert            | false  | false      |
   When a GET request is sent to [/client-api/members/me/notifications]
   Then the response status should be Ok
    And a list should have 2 item

Scenario: Customer wants to get all his UNREAD notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead |
    | 00          | alert            | false  |
    | 01          | alert            | true   |
    | 02          | alert            | false  |
   When a GET request is sent to [/client-api/members/me/notifications?isRead=false]
   Then the response status should be Ok
    And a list should have 2 items

Scenario: Customer wants to get all his READ notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead |
    | 00          | alert            | false  |
    | 01          | alert            | true   |
   When a GET request is sent to [/client-api/members/me/notifications?isRead=true]
   Then the response status should be Ok
    And a list should have 1 item

Scenario: Customer wants to get all his ARCHIVED notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead | IsArchived |
    | 00          | alert            | false  | false      |
    | 01          | alert            | false  | true       |
    | 02          | alert            | false  | false      |
   When a GET request is sent to [/client-api/members/me/notifications?isArchived=true]
   Then the response status should be Ok
    And a list should have 1 item

Scenario: Customer wants to get all his RESERVATION notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead |
    | 00          | alert            | false  |
    | 01          | reservation      | false  |
   When a GET request is sent to [/client-api/members/me/notifications?type=reservation]
   Then the response status should be Ok
    And a list should have 1 item

Scenario: Customer wants to get all his RESERVATION and UNREAD notifications
  Given with these notifications in database
    | Description | NotificationType | IsRead |
    | 00          | alert            | false  |
    | 01          | reservation      | false  |
    | 02          | reservation      | true   |
    | 03          | reservation      | true   |
   When a GET request is sent to [/client-api/members/me/notifications?type=reservation&isRead=false]
   Then the response status should be Ok
    And a list should have 1 item

Scenario: Customer wants to add a notification
  Given a payload request
     | notificationType | operatorId                           | caseId                               | description | data  |
     | alert            | 00E0A000-0000-0000-0001-000000000011 | CA110000-0000-0000-0001-000000000001 | some data   | extra |
   When a POST request is sent to [/client-api/members/me/notifications]
   Then the response status should be Created
    And member-notification record was updated

Scenario: Customer wants to get one notification
  Given a notification record exists
   When a GET request is sent to [/client-api/members/me/notifications/{notificationId}]
   Then the response should be successful

Scenario Outline: Customer wants to mark one notification
  Given a notification record exists
   When a PUT request is sent to [/client-api/members/me/notifications/{notificationId}/<readStatus>]
   Then the response should be successful
    And member-notification record was marked [<expected>]

    Examples:
    | readStatus | expected |
    | read       | true     |
    | unread     | false    |

Scenario: Customer wants to update a notification
  Given a notification record exists
  Given a payload request
     | notificationType | description |
     | alert            | some data   |
   When a PUT request is sent to [/client-api/members/me/notifications/{notificationId}]
   Then the response status should be Accepted

Scenario: Customer wants to delete a notification
  Given a notification record exists
   When a DELETE request is sent to [/client-api/members/me/notifications/{notificationId}]
   Then the response status should be Accepted
    And member-notification record was marked as archived
