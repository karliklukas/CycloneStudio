module cBCD_DECODER( input wire [3:0] BCD, input wire DOT, output wire [7:0] SEG);

//always block for converting bcd digit into 7 segment format
    always @(BCD)
    begin
        case (BCD) //case statement
            0 : SEG = {DOT,7'b0000001};
            1 : SEG = {DOT,7'b1001111};
            2 : SEG = {DOT,7'b0010010};
            3 : SEG = {DOT,7'b0000110};
            4 : SEG = {DOT,7'b1001100};
            5 : SEG = {DOT,7'b0100100};
            6 : SEG = {DOT,7'b0100000};
            7 : SEG = {DOT,7'b0001111};
            8 : SEG = {DOT,7'b0000000};
            9 : SEG = {DOT,7'b0000100};
            //switch off 7 segment character when the bcd digit is not a decimal number.
            default : SEG = {DOT,7'b1111111}; 
        endcase
    end
    
endmodule
