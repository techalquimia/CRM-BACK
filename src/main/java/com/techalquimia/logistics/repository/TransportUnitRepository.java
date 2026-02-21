package com.techalquimia.logistics.repository;

import com.techalquimia.logistics.entity.TransportUnit;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface TransportUnitRepository extends JpaRepository<TransportUnit, String> {

    Optional<TransportUnit> findByPlaca(String placa);

    List<TransportUnit> findByActivoTrueOrderByPlaca();
}
