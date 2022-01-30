// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/math/SafeMath.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/utils/Strings.sol";

//import "hardhat/console.sol";

contract SpideyReward is ERC721, Ownable {
  using SafeMath for uint256;
  using Counters for Counters.Counter;
  using Strings for uint256;

  string public tokenBaseURI;

  string public tokenUnrevealedURI;

  bool public mintActive = false;

  Counters.Counter public tokenSupply;

  uint256 public price = 0;

  uint256 public totalSupply = 500;

  constructor() ERC721("SPIDEY REWARD", "SRW") {
  }

  /************
   * Metadata *
   ************/

  function setTokenBaseURI(string memory baseURI) external onlyOwner {
    tokenBaseURI = baseURI;
  }

  function setTokenUnrevealedURI(string memory unrevealedURI) external onlyOwner {
    tokenUnrevealedURI = unrevealedURI;
  }


    function _baseURI() internal view override returns (string memory) {
        return tokenBaseURI;
    }

    function tokenURI(uint256 tokenId) public view override returns (string memory) {
        require(_exists(tokenId), "ERC721Metadata: URI query for nonexistent token");

        string memory baseURI = _baseURI();

        if ( bytes(baseURI).length > 0 )
        {
            return string(abi.encodePacked(baseURI, tokenId.toString()));
        }

        if (bytes(tokenUnrevealedURI).length > 0)
        {
            return tokenUnrevealedURI;
        }

        return "";
        
    }

  /********
   * Mint *
   ********/

  function mint() external payable 
  {
    require(mintActive, "Sale is not active.");
    require(msg.value == price, "The value sent is not correct");
    tokenSupply.increment();
    uint mintIndex = tokenSupply.current();
    require(mintIndex <= totalSupply, "Total supply has been minted");    
    _safeMint(msg.sender, tokenSupply.current());
  }

  /**************
   * Admin Only *
   **************/

  function setMintActive(bool _active) external onlyOwner {
    mintActive = _active;
  }

  function setPrice(uint256 newPrice) public onlyOwner {
  	price = newPrice;
  }

  function withdraw() public onlyOwner {
    uint balance = address(this).balance;
    payable(msg.sender).transfer(balance);
  }

}