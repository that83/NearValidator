CREATE TABLE [dbo].[validator]
(
    [date] DATETIME NOT NULL, 
    [account_id] CHAR(100) NOT NULL, 
    [stake] DECIMAL NOT NULL, 
    [near_price] DECIMAL NULL, 
    [num_expected_blocks] NUMERIC NULL, 
    [num_expected_chunks] NUMERIC NULL, 
    [num_produced_blocks] NUMERIC NULL, 
    [num_produced_chunks] NUMERIC NULL, 
    PRIMARY KEY ([date],[account_id])
)