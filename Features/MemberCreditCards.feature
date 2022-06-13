@CI @web @mongoDb @nonparallel
Feature: MemberCreditCards
    As a regular member
    I want to manage my credit cards

Background:
  Given a user with role [Customer] using a mobile client
    And a credit card exists

Scenario: Customer creates a credit card info
   Given a payload request
         | stripeId | last4 | name  | exp_month | exp_year | brand | address_zip | address_city | address_country | address_line1 | address_state | country | isDefault | stripeCustomerId |
         | abc1234  | 9998  | cardA | 3         | 2027     | VI    | H1H1H1      | Montreal     | CA              | line1         | QC            | CA      | true      | AAAAAAA11111     |
   When a POST request is sent to [/client-api/members/me/creditcards]
   Then the response should be successful

Scenario: Customer updates a credit card info
   Given a payload request
         | exp_month | exp_year | isDefault |
         | 3         | 2027     | true      |
   When a PUT request is sent to [/client-api/members/me/creditcards/{cCardId}]
   Then the response should be successful

Scenario: Customer deletes a credit card info
   When a DELETE request is sent to [/client-api/members/me/creditcards/{cCardId}]
   Then the response should be successful

Scenario: Customer gets credit card info
   When a GET request is sent to [/client-api/members/me/creditcards/{cCardId}]
   Then the response should be successful

Scenario: Customer gets all credit card info
   When a GET request is sent to [/client-api/members/me/creditcards/]
   Then the response should be successful
