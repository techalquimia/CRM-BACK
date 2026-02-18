package com.techalquimia.logistics.repository;

import com.techalquimia.logistics.entity.TicketWeight;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface TicketWeightRepository extends JpaRepository<TicketWeight, UUID> {
}
