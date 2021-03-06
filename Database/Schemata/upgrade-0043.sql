ALTER TABLE `FinancialTransactionRows` 
DROP COLUMN `Amount`


#


DROP PROCEDURE IF EXISTS `CreateFinancialTransactionRow`


#


DROP PROCEDURE IF EXISTS `CreateFinancialTransactionRowPrecise`


#


CREATE PROCEDURE `CreateFinancialTransactionRow`(
  IN financialTransactionId INTEGER,
  IN financialAccountId INTEGER,
  IN amountCents BIGINT,
  IN dateTime DATETIME,
  IN personId INTEGER
)
BEGIN

  INSERT INTO FinancialTransactionRows (FinancialAccountId, FinancialTransactionId, AmountCents, CreatedDateTime, CreatedByPersonId)
    VALUES (financialAccountId, financialTransactionid, amountCents, dateTime, personId);

  SELECT LAST_INSERT_ID() AS Identity;

END
