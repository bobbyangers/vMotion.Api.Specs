@CI @web @mongoDb @nonparallel
Feature: CallAgentQueries
    As a call agent
    I want to query the call queues

Background:
  Given an operator exists
  Given a user with role [Agent] using an http web client
   And  purge all calls

Scenario: A call agent queries for calls in-progress
  Given with these calls in database
    | Summary | OngoingCallBy | InVideoCallStatus | AssignedToUserType | ScheduledIn | AssignedTo |
    | 00      | $null$        |                   |                    |             |            |
    | 01      | $random$      | Active            | Agent              |             |            |
    | 02      | $null$        |                   |                    | 03:00:00:00 |            |
    | 03      | $null$        |                   | Agent              | 04:00:00:00 | $random$   |

  When a GET request is sent to [/api/calls/queue?qtype=inprogress]
  Then the response should be successful
    And a custom list should have 1 item


Scenario: A call agent queries for calls waiting
  Given with these calls in database
    | Summary | OngoingCallBy | InVideoCallStatus | AssignedToUserType | ScheduledIn | AssignedTo |
    | 00      | $null$        | Waiting           | Agent              |             | $null$     |
    | 01      | $random$      | Active            |                    |             | $null$     |
    | 02      | $null$        | Waiting           | Agent              | 3:00:00:00  | $null$     |
    | 03      | $null$        | Waiting           | Agent              |             | $random$   |

  When a GET request is sent to [/api/calls/queue?qtype=waiting]
  Then the response should be successful
   And a custom list should have 2 items

@ignore
Scenario: A call agent queries for calls waiting assigned to me
  Given with these calls in database
    | Summary | OngoingCallBy | InVideoCallStatus | AssignedToUserType | ScheduledIn | AssignedTo |
    | 00      | $null$        | Waiting           |                    |             |            |
    | 01      | $random$      | Active            |                    |             |            |
    | 02      | $null$        | Waiting           | Agent              | 3:00:00:00  |            |
    | 03      | $null$        | Waiting           | Agent              |             | $me$       |

  When a GET request is sent to [/api/calls/queue?qtype=waiting&assignedTo={staffId}]
  Then the response should be successful
   And a custom list should have 1 item

@ignore
Scenario: A call agent queries for calls scheduled
  Given with these calls in database
    | Summary | OngoingCallBy | InVideoCallStatus | AssignedToUserType | ScheduledIn | AssignedTo |
    | 00      |               |                   |                    |             |            |
    | 01      | $random$      | Active            | Agent              |             |            |
    | 02      |               |                   | Agent              | 3:00:00:00  |            |
    | 03      |               |                   | Agent              |             | $random$   |

  When a GET request is sent to [/api/calls/queue?qtype=scheduled]
  Then the response should be successful
   And a custom list should have 1

@ignore
Scenario: A call agent queries for calls scheduled for me
  Given with these calls in database
    | Summary | OngoingCallBy | InVideoCallStatus | AssignedToUserType | ScheduledIn | AssignedTo |
    | 00      |               |                   |                    |             |            |
    | 01      | $random$      | Active            |                    |             |            |
    | 02      |               |                   | Agent              | 03:00:00:00 | $me$       |
    | 03      |               |                   | Agent              | 04:00:00:00 | $random$   |

  When a GET request is sent to [/api/calls/queue?qtype=scheduled]
  Then the response should be successful
    And a custom list should have 2
