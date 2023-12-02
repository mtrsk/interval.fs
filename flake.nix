{
  description = "Development Environment";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    devenv = {
      url = "github:cachix/devenv";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = inputs@{ self, devenv, nixpkgs, ... }:
    let
      # System types to support.
      supportedSystems = [ "x86_64-linux" "x86_64-darwin" "aarch64-darwin" ];

      # Helper function to generate an attrset '{ x86_64-linux = f "x86_64-linux"; ... }'.
      forAllSystems = nixpkgs.lib.genAttrs supportedSystems;

      # Nixpkgs instantiated for supported system types.
      nixpkgsFor = forAllSystems (system: import nixpkgs {
        inherit system;
      });
    in
    {
      devShells = forAllSystems (system:
        let
          pkgs = nixpkgsFor."${system}";
          inherit (pkgs)
            bash
            gnumake
            dotnetCorePackages;

          dotnet = with dotnetCorePackages; combinePackages [
            sdk_6_0
          ];

          dotnet_8 = with dotnetCorePackages; combinePackages [
            sdk_8_0
          ];
        in
        {
          # `nix develop`
          default = devenv.lib.mkShell {
            inherit inputs pkgs;
            modules = [
              ({ pkgs, lib, ... }: {
                packages = [
                  bash
                  gnumake
                ];

                languages.dotnet = {
                  enable = true;
                  package = dotnet;
                };

                # looks for the .env by default
                # additionaly, there is .filename
                # if an arbitrary file is desired
                dotenv.enable = true;
              })
            ];
          };
        });
    };
}
