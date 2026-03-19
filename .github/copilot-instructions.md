# GitHub Copilot Code Review Custom Instructions

## 🛑 MANDATORY BYPASS RULES
1. **Bot Exclusion Pattern**: 
   - IF the author of the Pull Request or commit has a username ending in "-bot" (e.g., GeneriDocs-bot, PushDocs-bot) or "-got" (e.g., Enque-got), DO NOT perform any review.
   - This also applies to system bots containing "[bot]" (e.g., github-actions[bot]).
   - Terminate the review process immediately without leaving any comments.

2. **Manual Keyword Exclusion**:
   - IF the Pull Request title, description, or any commit message contains the tag "[IGNORE-BOT]", STOP the review immediately.
   - Do not provide suggestions, security alerts, or code improvements if this tag is present.

3. **Branch Protection (main)**:
   - DO NOT review or comment on any direct changes or merges to the "main" branch if the commit message includes "[IGNORE-BOT]" or if the author is one of the recognized bots.

## 🤖 EXECUTION LOGIC
- Before analyzing any code or diff, check the author's name and the presence of "[IGNORE-BOT]".
- If a bypass condition is met, your only response should be: "Review bypassed per repository instructions." and then exit.