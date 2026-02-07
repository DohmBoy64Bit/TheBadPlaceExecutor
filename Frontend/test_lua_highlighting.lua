-- This is a comment to test syntax highlighting
--[[ This is a 
     multiline comment ]]

local function testFunction(param1, param2)
    -- Test keywords
    if param1 then
        print("String in double quotes")
        local value = 'String in single quotes'
        return true
    elseif param2 then
        for i = 1, 10 do
            print(i)
        end
    else
        return false
    end
end

-- Test operators
local x = 10 + 5 - 3 * 2 / 1
local y = x % 2
local z = 2 ^ 3

-- Test built-in functions
local t = {1, 2, 3}
for k, v in pairs(t) do
    print(tostring(v))
end

-- Test nil, true, false
local a = nil
local b = true
local c = false

-- Test multiline string
local multilineString = [[
This is a
multiline string
]]

-- Test while and repeat
while x > 0 do
    x = x - 1
end

repeat
    y = y + 1
until y > 10
